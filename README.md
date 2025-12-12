VIDEO DE YOUTUBE: https://youtu.be/cB9RMAZxMSI?si=bDdh96vWKxKWGt9y
LINK POSTMAN: https://juandavidtapia123-2928581.postman.co/workspace/Juan-David-Tapia-Frias's-Worksp~c2bd8053-4335-455e-8728-dd3c0e99ca25/collection/49091229-63f08a63-1f70-4bc7-83a2-051b13035958?action=share&source=copy-link&creator=49091229



Caja San Miguel API (Backend)
Descripción del Proyecto
El proyecto Caja San Miguel API es el núcleo backend de una plataforma integral diseñada para la gestión de una entidad financiera o caja de ahorro. Desarrollado con .NET Core (C#) y Entity Framework Core, este sistema proporciona una arquitectura robusta y escalable para la administración de préstamos, clientes y pagos.
Propósito: El objetivo principal es automatizar y optimizar los procesos financieros críticos, eliminando la gestión manual y proporcionando una fuente única de verdad para la toma de decisiones.
Funcionalidad Principal:
Motor de Créditos Inteligente: Cálculo automático de intereses y generación de tablas de amortización (calendarios de pago) al momento de crear un préstamo.
Gestión de Cobranza: Registro y validación de pagos individuales con actualización en tiempo real de saldos.
Auditoría Automática (Lazy Update): Algoritmos que verifican vencimientos y aplican multas moratorias automáticamente al consultar la información, asegurando que los estados financieros estén siempre al día.
Seguridad: Autenticación y autorización mediante JWT (JSON Web Tokens).
Endpoints Implementados Destacados:
POST /api/Auth/login: Autenticación segura y emisión de tokens.
POST /api/Prestamo: Creación de préstamos con proyección financiera automática.
GET /api/Prestamo: Contiene una lógica donde compara las fechas de los pagos y aplicando multas.
PATCH /api/Prestamo/{id}: Permite modificar las condiciones de un préstamo existente, recalcando los datos.
PATCH /api/Pago/{id}: Utilizado para pagar las cuotas






 Instrucciones para Ejecutar el Proyecto
Sigue estos pasos para clonar, configurar y ejecutar el servidor backend en tu entorno local.
Requisitos del Sistema
.NET SDK: Versión 8.0 o superior (compatible con .NET Core).
Base de Datos: Microsoft SQL Server (LocalDB o instancia completa).
Pasos de Instalación

Clonar el Repositorio:
git clone https://github.com/DavidTapiaLOl/CajaSanmiguelWebApi.git
cd CajaSanmiguelWebApi

Restaurar Dependencias: Ejecuta el siguiente comando en la raíz del proyecto para descargar los paquetes NuGet necesarios:

dotnet restore
Configuración Inicial
Configurar Cadena de Conexión: Abre el archivo appsettings.json y asegúrate de que la cadena de conexión apunte a tu instancia local de SQL Server.

"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=CajaSanMiguelDB;Trusted_Connection=True;MultipleActiveResultSets=true"
}

Configurar JWT (Seguridad): En el mismo archivo appsettings.json, verifica o configura la clave secreta para la firma de tokens:

"Jwt": {
  "Key": "TuClaveSecretaSuperSeguraYLargaParaFirma",
  "Issuer": "http://localhost:5054",
  "Audience": "http://localhost:5054"
}




Crear la Base de Datos (Migraciones): El proyecto utiliza Code First. Para crear la base de datos y las tablas, ejecuta:

dotnet ef database update
(Nota: Si no tienes instalado EF Core globalmente, instálalo con: dotnet tool install --global dotnet-ef)
Iniciar el Proyecto
Una vez configurado, puedes iniciar el servidor con el siguiente comando:
dotnet run --launch-profile https



