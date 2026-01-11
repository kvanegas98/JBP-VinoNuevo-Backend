# Sistema de Gestión - Vino Nuevo JBP

Sistema de gestión académica y financiera para instituto teológico.

## 🚀 Configuración Inicial

### 1. Clonar el repositorio
```bash
git clone [URL_DEL_REPOSITORIO]
cd "Vino Nuevo JBP Backend"
```

### 2. Configurar la base de datos

**Opción A: Copiar el archivo de configuración**
```bash
cd Sistema.Web
cp appsettings.Example.json appsettings.json
```

**Opción B: Crear manualmente `appsettings.json`** con el siguiente contenido:

```json
{
  "ConnectionStrings": {
    "Conexion": "Data Source=YOUR_SQL_SERVER;Initial Catalog=YOUR_DATABASE;User Id=YOUR_USERNAME;Password=YOUR_PASSWORD;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Jwt": {
    "Key": "YOUR_SECRET_KEY_HERE_AT_LEAST_32_CHARACTERS_LONG",
    "Issuer": "https://localhost:44388/"
  }
}
```

⚠️ **IMPORTANTE**: Nunca subas el archivo `appsettings.json` a Git. Ya está incluido en `.gitignore`.

### 3. Restaurar paquetes NuGet
```bash
dotnet restore
```

### 4. Ejecutar migraciones (si aplica)
```bash
dotnet ef database update
```

### 5. Ejecutar el proyecto
```bash
cd Sistema.Web
dotnet run
```

El servidor estará disponible en:
- HTTPS: `https://localhost:5001`
- HTTP: `http://localhost:5000`

## 📁 Estructura del Proyecto

```
Vino Nuevo JBP Backend/
├── Sistema.Datos/          # Capa de acceso a datos (DbContext, Mapping)
├── Sistema.Entidades/      # Entidades del dominio
├── Sistema.Web/            # API REST (Controllers, Models)
│   ├── Controllers/        # Endpoints de la API
│   ├── Models/             # ViewModels para requests/responses
│   └── appsettings.json    # ⚠️ NO SUBIR A GIT
└── Scripts/                # Scripts SQL útiles
```

## 🔑 Módulos Principales

- **Autenticación y Usuarios**: Login con JWT, gestión de usuarios y roles
- **Estudiantes**: Registro de estudiantes internos/externos, becas
- **Matrículas**: Inscripción a módulos y materias
- **Pagos**: Gestión de pagos de matrícula y mensualidades
- **Notas**: Registro de calificaciones por materia
- **Reportes**: Dashboard, morosidad, reportes financieros

## 🛠️ Tecnologías

- **.NET Core 2.1** / ASP.NET Core
- **Entity Framework Core** (Code First)
- **SQL Server**
- **JWT** para autenticación
- **CORS** habilitado

## 📖 Documentación de Endpoints

Ver archivo `FRONTEND_PROMPT_AUTHENTICATION.md` para documentación completa de los endpoints de autenticación y usuarios.

## 🗄️ Scripts SQL Útiles

### Limpiar datos de prueba
```bash
# Ubicación: Scripts/LimpiarEstudiantesMatriculasPagos.sql
# Este script elimina todos los estudiantes, matrículas, pagos y notas
# ⚠️ USAR CON PRECAUCIÓN - Respaldar antes de ejecutar
```

## 🔒 Seguridad

- Las contraseñas se almacenan con hash HMACSHA512 + salt
- Los tokens JWT expiran en 24 horas
- CORS configurado para aceptar todos los orígenes (ajustar en producción)

## 🚧 Pendientes / Mejoras Futuras

- [ ] Implementar refresh tokens
- [ ] Agregar rate limiting
- [ ] Implementar permisos granulares por módulo
- [ ] Agregar logs de auditoría
- [ ] Implementar autenticación de dos factores (2FA)

## 📝 Notas para Desarrollo

### Crear un nuevo usuario inicial (SQL)
```sql
-- Ejecutar después de crear la base de datos
-- La contraseña será: "Admin123"
-- Hash generado con HMACSHA512
```

### Configuración de CORS
Por defecto acepta todos los orígenes. En `Startup.cs`:
```csharp
services.AddCors(options => {
    options.AddPolicy("Todos",
    builder => builder.WithOrigins("*").WithHeaders("*").WithMethods("*"));
});
```

En producción, cambiar `"*"` por los dominios específicos.

## 🤝 Contribuir

1. Crear una rama nueva: `git checkout -b feature/nueva-funcionalidad`
2. Hacer commit de los cambios: `git commit -m 'Agregar nueva funcionalidad'`
3. Push a la rama: `git push origin feature/nueva-funcionalidad`
4. Abrir un Pull Request

## 📧 Contacto

[Tu información de contacto o del equipo]

---

**⚠️ Recordatorios importantes:**
- Nunca subir `appsettings.json` a Git
- Hacer respaldo de la base de datos antes de ejecutar scripts de limpieza
- Cambiar la clave JWT en producción
- Configurar CORS específico en producción
