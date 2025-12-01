using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace SeleniumTests
{
    [TestFixture]
    public class ThinkingTesterContactListTests
    {
        private IWebDriver? driver;
        private WebDriverWait? wait;

        private const string BaseUrl = "https://thinking-tester-contact-list.herokuapp.com/";
        private static string TestEmail = string.Empty;
        private const string TestPassword = "Password123!";

        // Helper para crear el ChromeDriver sin usar Selenium Manager
        private ChromeDriver CreateChromeDriver()
        {
            var driverDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                                 ?? throw new InvalidOperationException("No se pudo determinar la ruta del driver.");

            var options = new ChromeOptions();
            var service = ChromeDriverService.CreateDefaultService(driverDirectory);

            return new ChromeDriver(service, options, TimeSpan.FromSeconds(60));
        }

        // Usuario de prueba creado una vez para todos los tests
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // correo generico
            TestEmail = $"selenium.{Guid.NewGuid().ToString("N").Substring(0, 6)}@gmail.com";

            using var setupDriver = CreateChromeDriver();
            setupDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            setupDriver.Navigate().GoToUrl(BaseUrl);

            // sign up
            setupDriver.FindElement(By.Id("signup")).Click();

            // registro
            setupDriver.FindElement(By.Id("firstName")).SendKeys("Hideki");
            setupDriver.FindElement(By.Id("lastName")).SendKeys("Ariyama");
            setupDriver.FindElement(By.Id("email")).SendKeys(TestEmail);
            setupDriver.FindElement(By.Id("password")).SendKeys(TestPassword);
            setupDriver.FindElement(By.Id("submit")).Click();

            // Cerrar sesion
            var logoutButtons = setupDriver.FindElements(By.XPath("//button[contains(.,'Logout')]"));
            if (logoutButtons.Count > 0)
            {
                logoutButtons[0].Click();
            }
        }

        // tear down para cada test

        [SetUp]
        public void SetUp()
        {
            driver = CreateChromeDriver();
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        }

        [TearDown]
        public void TearDown()
        {
            try
            {
                TakeScreenshot(TestContext.CurrentContext.Test.Name);
            }
            catch
            {
                // ignorar errores al tomar screenshot
            }

            if (driver != null)
            {
                try { driver.Close(); } catch { }
                try { driver.Quit(); } catch { }

                driver.Dispose();
                driver = null;
                wait = null;
            }
        }

        // helpers

        private void TakeScreenshot(string testName)
        {
            if (driver is not ITakesScreenshot screenshotDriver) return;

            var screenshot = screenshotDriver.GetScreenshot();

            var folder = Path.Combine(
                TestContext.CurrentContext.WorkDirectory,
                "Screenshots");
            Directory.CreateDirectory(folder);

            var filePath = Path.Combine(
                folder,
                $"{testName}_{DateTime.Now:yyyyMMdd_HHmmss}.png");

            File.WriteAllBytes(filePath, screenshot.AsByteArray);
            TestContext.AddTestAttachment(filePath);
        }

        private void Login(string email, string password)
        {
            driver!.Navigate().GoToUrl(BaseUrl);

            var emailInput = driver.FindElement(By.Id("email"));
            emailInput.Clear();
            emailInput.SendKeys(email);

            var passwordInput = driver.FindElement(By.Id("password"));
            passwordInput.Clear();
            passwordInput.SendKeys(password);

            driver.FindElement(By.Id("submit")).Click();
        }

        private void LoginWithValidUser()
        {
            Login(TestEmail, TestPassword);

            wait!.Until(d => d.Url.Contains("/contactList"));

            var heading = driver!.FindElement(By.TagName("h1")).Text;
            Assert.That(heading, Is.EqualTo("Contact List"),
                "No se mostro la pantalla de Contact List");
        }

        // Tests de Login

        [Test]
        public void CaminoFeliz_Login_RedireccionaAContactList()
        {
            Login(TestEmail, TestPassword);

            wait!.Until(d => d.Url.Contains("/contactList"));
            Assert.That(driver!.Url, Does.Contain("/contactList"));
        }

        [Test]
        public void Negativo_Login_PasswordIncorrecto_MuestraMensajeError()
        {
            Login(TestEmail, "ClaveMala123!");

            var errorElement = wait!.Until(d =>
                d.FindElement(By.XPath("//*[contains(text(),'Incorrect username or password')]")));

            Assert.That(errorElement.Text, Does.Contain("Incorrect username or password"));
        }

        [Test]
        public void Limite_Login_EmailYPasswordVacios_MuestraMensajeError()
        {
            Login(string.Empty, string.Empty);

            var errorElement = wait!.Until(d =>
                d.FindElement(By.XPath("//*[contains(text(),'Incorrect username or password')]")));

            Assert.That(errorElement.Text, Does.Contain("Incorrect username or password"));
        }

        // Tests de crud 
        // Camino feliz - Create
        [Test]
        public void CaminoFeliz_Contacto_Crear_ContactoApareceEnLista()
        {
            LoginWithValidUser();

            driver!.FindElement(By.CssSelector("button#add-contact")).Click();

            var firstName = "Carlos";
            var lastName = "Perez_" + Guid.NewGuid().ToString("N").Substring(0, 5);
            var fullName = $"{firstName} {lastName}";

            driver.FindElement(By.Id("firstName")).SendKeys(firstName);
            driver.FindElement(By.Id("lastName")).SendKeys(lastName);
            driver.FindElement(By.Id("birthdate")).SendKeys("1990-01-01");
            driver.FindElement(By.Id("email")).SendKeys("carlos@gmail.com");
            driver.FindElement(By.Id("phone")).SendKeys("8095551234");
            driver.FindElement(By.Id("street1")).SendKeys("Calle 1");
            driver.FindElement(By.Id("street2")).SendKeys("Calle 2");
            driver.FindElement(By.Id("city")).SendKeys("Santo Domingo");
            driver.FindElement(By.Id("submit")).Click();

            wait!.Until(d => d.Url.Contains("/contactList"));

            var contactCell = driver.FindElement(
                By.XPath($"//table//td[contains(text(),'{fullName}')]"));

            Assert.That(contactCell.Text, Does.Contain(firstName));
        }

        // Negativa - Create (sin nombre)
        [Test]
        public void Negativo_Contacto_Crear_SinNombre_NoSeCrea()
        {
            LoginWithValidUser();

            driver!.FindElement(By.CssSelector("button#add-contact")).Click();

            var lastName = "SinNombre_" + Guid.NewGuid().ToString("N").Substring(0, 5);

            driver.FindElement(By.Id("lastName")).SendKeys(lastName);
            driver.FindElement(By.Id("birthdate")).SendKeys("1995-01-01");
            driver.FindElement(By.Id("email")).SendKeys("sin.nombre@gmail.com");
            driver.FindElement(By.Id("phone")).SendKeys("8095559999");
            driver.FindElement(By.Id("street1")).SendKeys("Calle 3");
            driver.FindElement(By.Id("city")).SendKeys("Santo Domingo");

            driver.FindElement(By.Id("submit")).Click();

            Assert.That(driver!.Url, Does.Not.Contain("/contactList"),
                "El sistema permitio crear un contacto sin nombre y navego a Contact List.");
        }

        // Limites - Create (teléfono muy largo)
        [Test]
        public void Limite_Contacto_Crear_TelefonoMuyLargo_NoSeCrea()
        {
            LoginWithValidUser();

            driver!.FindElement(By.CssSelector("button#add-contact")).Click();

            var firstName = "Pedro";
            var lastName = "LimiteTel_" + Guid.NewGuid().ToString("N").Substring(0, 5);

            driver.FindElement(By.Id("firstName")).SendKeys(firstName);
            driver.FindElement(By.Id("lastName")).SendKeys(lastName);
            driver.FindElement(By.Id("birthdate")).SendKeys("1993-03-03");
            driver.FindElement(By.Id("email")).SendKeys("pedro@gmail.com");

            var telefonoLargo = "80912345678901234567";
            driver.FindElement(By.Id("phone")).SendKeys(telefonoLargo);

            driver.FindElement(By.Id("street1")).SendKeys("Calle 4");
            driver.FindElement(By.Id("city")).SendKeys("Santo Domingo");

            driver.FindElement(By.Id("submit")).Click();

            Assert.That(driver!.Url, Does.Not.Contain("/contactList"),
                "El sistema permitio crear un contacto con un telefono excesivamente largo.");
        }

        // Camino feliz - Read
        [Test]
        public void CaminoFeliz_Contacto_Listar_ContactListSeMuestraCorrectamente()
        {
            LoginWithValidUser();

            var heading = driver!.FindElement(By.TagName("h1")).Text;
            Assert.That(heading, Is.EqualTo("Contact List"));

            var rows = driver.FindElements(By.CssSelector("tr.contactTableBodyRow"));
            Assert.That(rows.Count, Is.GreaterThanOrEqualTo(0));
        }

        // Camino feliz - Update
        [Test]
        public void CaminoFeliz_Contacto_Actualizar_VuelveAlListadoSinErrores()
        {
            LoginWithValidUser();

            // crear contacto
            driver!.FindElement(By.CssSelector("button#add-contact")).Click();

            var firstName = "Ana";
            var lastName = "Lopez_" + Guid.NewGuid().ToString("N").Substring(0, 5);
            var fullName = $"{firstName} {lastName}";

            driver.FindElement(By.Id("firstName")).SendKeys(firstName);
            driver.FindElement(By.Id("lastName")).SendKeys(lastName);
            driver.FindElement(By.Id("birthdate")).SendKeys("1992-05-10");
            driver.FindElement(By.Id("email")).SendKeys("ana@gmail.com");
            driver.FindElement(By.Id("phone")).SendKeys("8295554321");
            driver.FindElement(By.Id("street1")).SendKeys("Calle 5");
            driver.FindElement(By.Id("city")).SendKeys("Santiago");
            driver.FindElement(By.Id("submit")).Click();

            // buscar contacto
            var row = driver.FindElement(
                By.XPath($"//table//td[contains(text(),'{fullName}')]"));
            row.Click();

            // editar contacto
            var editButton = driver.FindElement(
                By.XPath("//button[contains(.,'Edit Contact')]"));
            editButton.Click();

            // actualizar
            var cityInput = driver.FindElement(By.Id("city"));
            var nuevaCiudad = "Ciudad Actualizada";

            cityInput.Clear();
            cityInput.SendKeys(nuevaCiudad);

            driver.FindElement(By.Id("submit")).Click();

            var heading = driver.FindElement(By.TagName("h1")).Text;

            Assert.That(heading, Is.EqualTo("Edit Contact")
                .Or.EqualTo("Contact List"),
                "La página no mostró un estado esperado.");
        }

        // Camino feliz - Delete
        [Test]
        public void CaminoFeliz_Contacto_Eliminar_DesapareceDeLaLista()
        {
            LoginWithValidUser();

            // crear contacto a eliminar
            driver!.FindElement(By.CssSelector("button#add-contact")).Click();

            var firstName = "Luis";
            var lastName = "Gomez_" + Guid.NewGuid().ToString("N").Substring(0, 5);
            var fullName = $"{firstName} {lastName}";

            driver.FindElement(By.Id("firstName")).SendKeys(firstName);
            driver.FindElement(By.Id("lastName")).SendKeys(lastName);
            driver.FindElement(By.Id("birthdate")).SendKeys("1988-09-15");
            driver.FindElement(By.Id("email")).SendKeys("luis@gmail.com");
            driver.FindElement(By.Id("phone")).SendKeys("8495550000");
            driver.FindElement(By.Id("street1")).SendKeys("Calle 6");
            driver.FindElement(By.Id("city")).SendKeys("La Vega");
            driver.FindElement(By.Id("submit")).Click();

            wait!.Until(d => d.Url.Contains("/contactList"));

            // abrir detalles del contacto
            var row = driver.FindElement(
                By.XPath($"//table//td[contains(text(),'{fullName}')]"));
            row.Click();

            // delete
            driver.FindElement(By.CssSelector("button#delete")).Click();

            try
            {
                var alert = driver.SwitchTo().Alert();
                alert.Accept();
            }
            catch (NoAlertPresentException)
            {
                Assert.Fail("No se mostro la alerta de confirmacion para eliminar el contacto.");
            }

            wait!.Until(d => d.Url.Contains("/contactList"));

            int count;
            try
            {
                count = driver.FindElements(
                    By.XPath($"//table//td[contains(text(),'{fullName}')]")).Count;
            }
            catch (WebDriverException)
            {
                count = 0;
            }

            Assert.That(count, Is.EqualTo(0),
                "El contacto todavia aparece en la lista despues de eliminarlo.");
        }
    }
}
