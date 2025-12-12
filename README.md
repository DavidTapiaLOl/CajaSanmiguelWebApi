# Caja San Miguel API (Backend)

El proyecto **Caja San Miguel API** es el núcleo backend de una plataforma integral diseñada para la gestión de una entidad financiera o caja de ahorro. Desarrollado con **.NET Core (C#)** y **Entity Framework Core**, este sistema proporciona una arquitectura robusta y escalable para la administración de préstamos, clientes y pagos.

## 🔗 Recursos

- **Video Demo:** [Ver en YouTube](https://youtu.be/cB9RMAZxMSI?si=bDdh96vWKxKWGt9y)
- **Documentación API:** [Postman Collection](https://juandavidtapia123-2928581.postman.co/workspace/Juan-David-Tapia-Frias's-Worksp~c2bd8053-4335-455e-8728-dd3c0e99ca25/collection/49091229-63f08a63-1f70-4bc7-83a2-051b13035958?action=share&source=copy-link&creator=49091229)

---

## 🎯 Propósito

El objetivo principal es automatizar y optimizar los procesos financieros críticos, eliminando la gestión manual y proporcionando una fuente única de verdad para la toma de decisiones.

## 🚀 Funcionalidad Principal

- **Motor de Créditos Inteligente:** Cálculo automático de intereses y generación de tablas de amortización.
- **Gestión de Cobranza:** Registro y validación de pagos con actualización en tiempo real.
- **Auditoría Automática (Lazy Update):** Verificación de vencimientos y aplicación de multas automática al consultar.
- **Seguridad:** Autenticación mediante JWT.

---

## 📡 Endpoints Destacados

| Método | Endpoint | Descripción |
| :--- | :--- | :--- |
| `POST` | `/api/Auth/login` | Autenticación y token. |
| `POST` | `/api/Prestamo` | Creación de préstamos. |
| `GET` | `/api/Prestamo` | Consulta con aplicación de multas automática. |
| `PATCH` | `/api/Prestamo/{id}` | Re-cálculo de condiciones de préstamo. |
| `PATCH` | `/api/Pago/{id}` | Registro de pagos. |

---

## 💻 Instrucciones de Instalación

Sigue estos pasos para ejecutar el backend localmente.

### 1. Requisitos Previos
- .NET SDK 8.0 o superior.
- SQL Server (LocalDB o instancia completa).

### 2. Instalación y Ejecución

**Paso 1: Clonar y entrar al directorio**
```bash
git clone [https://github.com/DavidTapiaLOl/CajaSanmiguelWebApi.git](https://github.com/DavidTapiaLOl/CajaSanmiguelWebApi.git)
cd CajaSanmiguelWebApi

Paso 2: Restaurar dependencias
dotnet restore

Paso 3: Configuración (appsettings.json) Asegúrate de configurar tu ConnectionStrings y Jwt en el archivo appsettings.json:
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=CajaSanMiguelDB;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "Jwt": {
    "Key": "TuClaveSecretaSuperSeguraYLargaParaFirma",
    "Issuer": "http://localhost:5054",
    "Audience": "http://localhost:5054"
  }
}

Paso 4: Base de Datos y Ejecución


# Crear base de datos
dotnet ef database update

# Iniciar servidor
dotnet run --launch-profile https
