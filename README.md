# Selenium-CRUD
Desarrollar pruebas automatizadas sobre una aplicación o página web, utilizando la  librería Selenium y cumpliendo con criterios técnicos, documentales y de presentación  en video. 


Test HTML
https://github.com/hid-ari/Selenium-CRUD/tree/014feed357071b2065d8b3623e5f396b98088813/Selenium/SeleniumTests/TestResults

Capturas del test 
https://github.com/hid-ari/Selenium-CRUD/tree/014feed357071b2065d8b3623e5f396b98088813/Selenium/SeleniumTests/bin/Debug/net9.0/TestResults/Screenshots

Video del test
https://youtu.be/JeYYPSZFYvw

Thinking Tester Contact List
https://thinking-tester-contact-list.herokuapp.com/

## Descripción general

Este proyecto contiene una suite de pruebas automatizadas desarrolladas con C#, .NET y Selenium WebDriver sobre la aplicación web pública:

`https://thinking-tester-contact-list.herokuapp.com/`

El objetivo es cumplir con los requisitos de la tarea de pruebas automatizadas:

- Automatizar el flujo de inicio de sesión.
- Automatizar operaciones CRUD sobre contactos.
- Incluir pruebas de:
  - Camino feliz
  - Prueba negativa
  - Prueba de límites
- Generar evidencias de ejecución (capturas de pantalla y reporte en HTML).

Todas las pruebas están implementadas en la clase:

`SeleniumTests.ThinkingTesterContactListTests`

## Tecnologías utilizadas

- .NET 9.0
- C#
- NUnit
- Selenium WebDriver
- Selenium.WebDriver.ChromeDriver
- Microsoft.NET.Test.Sdk
- NUnit3TestAdapter
- coverlet.collector (para cobertura de código, si se requiere)

## Diseño de la solución

### Usuario de prueba

Antes de ejecutar cualquier prueba, en el método `[OneTimeSetUp]` se realiza:

- Creación de un usuario de prueba en el sitio
  - Correo: generado dinámicamente.
  - Contraseña: `Password123!`
- Este usuario se utiliza en todos los casos de prueba que requieren autenticación.

### Setup y TearDown

Para cada prueba:

- `[SetUp]`:
  - Se crea una instancia de `ChromeDriver` mediante un helper `CreateChromeDriver()`.

- `[TearDown]`:
  - Se toma una captura de pantalla del estado final del navegador y se guarda en:
    - `TestResults/Screenshots/`
  - Se cierran y liberan los recursos del navegador (`Close`, `Quit`, `Dispose`).

### Capturas de pantalla

Cada prueba genera automáticamente una captura de pantalla con el nombre del test y marca de tiempo:

Estas capturas pueden usarse como evidencia en el reporte y en la presentación en video.

## Organización de las pruebas

Las pruebas se agrupan en dos bloques principales:

1. Pruebas de Login
2. Pruebas de CRUD de Contactos

### 1. Pruebas de Login

Clase: `ThinkingTesterContactListTests`

Métodos:

- `CaminoFeliz_Login_RedireccionaAContactList`
  - Verifica que, con credenciales válidas, el sistema redirige a la pantalla `Contact List`.

- `Negativo_Login_PasswordIncorrecto_MuestraMensajeError`
  - Usa una contraseña incorrecta y verifica el mensaje:
    - `Incorrect username or password`.

- `Limite_Login_EmailYPasswordVacios_MuestraMensajeError`
  - Intenta iniciar sesión con email y password vacíos.
  - Verifica que se muestra el mismo mensaje de error.

### 2. Pruebas CRUD de Contactos

#### Create

- `CaminoFeliz_Contacto_Crear_ContactoApareceEnLista`
  - Crea un contacto con datos válidos.

- `Negativo_Contacto_Crear_SinNombre_NoSeCrea`
  - Intenta crear un contacto dejando el campo nombre vacío.
  - Verifica que no se navega a `Contact List` después de enviar el formulario.

- `Limite_Contacto_Crear_TelefonoMuyLargo_NoSeCrea`
  - Intenta crear un contacto con un número de teléfono excesivamente largo.
  - Verifica que no se navega a `Contact List`.

#### Read

- `CaminoFeliz_Contacto_Listar_ContactListSeMuestraCorrectamente`
  - Verifica que, después del login, se muestra:
    - Encabezado `Contact List`.
    - Tabla de contactos (`tr.contactTableBodyRow`).

#### Update

- `CaminoFeliz_Contacto_Actualizar_VuelveAlListadoSinErrores`
  - Crea un contacto.
  - Abre su detalle, pulsa `Edit Contact`, modifica la ciudad y envía el formulario.
  - Verifica que el flujo termina en un estado válido (`Edit Contact` o `Contact List`).

#### Delete

- `CaminoFeliz_Contacto_Eliminar_DesapareceDeLaLista`
  - Crea un contacto.
  - Abre su detalle, pulsa `Delete`, acepta la alerta.
  - Verifica que el contacto ya no aparece en la lista de contactos.

## Requisitos

- SDK de .NET 9.0 instalado.
- Google Chrome instalado.
- Paquetes NuGet restaurados para el proyecto:
  - `Selenium.WebDriver`
  - `Selenium.WebDriver.ChromeDriver`
  - `Selenium.Support`
  - `NUnit`
  - `NUnit3TestAdapter`
  - `Microsoft.NET.Test.Sdk`

## Ejecución de las pruebas

Ubicarse en la carpeta del proyecto de pruebas (donde está el `.csproj`):

```bash
dotnet test -l:html -r TestResults

