using Microsoft.AspNetCore.Mvc;

namespace Sistema.Web.Controllers
{
    [ApiController]
    public class HomeController : ControllerBase
    {
        [HttpGet("")]
        [Route("")]
        public IActionResult RedirectToInfo()
        {
            return Redirect("/api/info");
        }

        [HttpGet("api/info")]
        public ContentResult Index()
        {
            var html = @"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Sistema Vino Nuevo JBP - API</title>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
            padding: 20px;
        }
        .container {
            max-width: 1200px;
            margin: 0 auto;
            background: white;
            border-radius: 15px;
            box-shadow: 0 20px 60px rgba(0,0,0,0.3);
            overflow: hidden;
        }
        .header {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 40px;
            text-align: center;
        }
        .header h1 {
            font-size: 2.5em;
            margin-bottom: 10px;
        }
        .header p {
            font-size: 1.2em;
            opacity: 0.9;
        }
        .content {
            padding: 40px;
        }
        .section {
            margin-bottom: 30px;
        }
        .section h2 {
            color: #667eea;
            margin-bottom: 15px;
            font-size: 1.8em;
            border-bottom: 3px solid #667eea;
            padding-bottom: 10px;
        }
        .section p {
            line-height: 1.6;
            color: #555;
            font-size: 1.1em;
        }
        .modules {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
            gap: 20px;
            margin-top: 20px;
        }
        .module-card {
            background: #f8f9fa;
            padding: 20px;
            border-radius: 10px;
            border-left: 4px solid #667eea;
            transition: transform 0.3s;
        }
        .module-card:hover {
            transform: translateY(-5px);
            box-shadow: 0 5px 15px rgba(102, 126, 234, 0.3);
        }
        .module-card h3 {
            color: #764ba2;
            margin-bottom: 10px;
        }
        .module-card p {
            color: #666;
            font-size: 0.95em;
        }
        .tech-stack {
            display: flex;
            flex-wrap: wrap;
            gap: 10px;
            margin-top: 15px;
        }
        .tech-badge {
            background: #667eea;
            color: white;
            padding: 8px 15px;
            border-radius: 20px;
            font-size: 0.9em;
            font-weight: 500;
        }
        .status {
            text-align: center;
            padding: 20px;
            background: #d4edda;
            border: 1px solid #c3e6cb;
            border-radius: 10px;
            margin-top: 30px;
        }
        .status-badge {
            display: inline-block;
            background: #28a745;
            color: white;
            padding: 10px 20px;
            border-radius: 25px;
            font-weight: bold;
            font-size: 1.1em;
        }
        .footer {
            background: #f8f9fa;
            padding: 20px;
            text-align: center;
            color: #666;
            border-top: 1px solid #dee2e6;
        }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎓 Sistema de Gestión Vino Nuevo JBP</h1>
            <p>API REST para Gestión Académica y Financiera</p>
        </div>

        <div class='content'>
            <div class='section'>
                <h2>📋 Descripción del Proyecto</h2>
                <p>
                    Sistema integral de gestión académica y financiera diseñado específicamente para el
                    Instituto Teológico Vino Nuevo JBP. Proporciona una solución completa para administrar
                    estudiantes, matrículas, pagos, calificaciones y generar reportes académicos y financieros.
                </p>
            </div>

            <div class='section'>
                <h2>🔑 Módulos Principales</h2>
                <div class='modules'>
                    <div class='module-card'>
                        <h3>👤 Autenticación y Usuarios</h3>
                        <p>Sistema de login con JWT, gestión de usuarios y roles con seguridad avanzada.</p>
                    </div>
                    <div class='module-card'>
                        <h3>📚 Gestión de Estudiantes</h3>
                        <p>Registro completo de estudiantes internos y externos, incluyendo gestión de becas.</p>
                    </div>
                    <div class='module-card'>
                        <h3>📝 Matrículas</h3>
                        <p>Control de inscripciones a módulos académicos y materias por período lectivo.</p>
                    </div>
                    <div class='module-card'>
                        <h3>💰 Sistema de Pagos</h3>
                        <p>Gestión de pagos de matrícula y mensualidades con control de morosidad.</p>
                    </div>
                    <div class='module-card'>
                        <h3>📊 Calificaciones</h3>
                        <p>Registro y consulta de notas académicas por materia y estudiante.</p>
                    </div>
                    <div class='module-card'>
                        <h3>📈 Reportes</h3>
                        <p>Dashboard ejecutivo, reportes de morosidad y análisis financiero detallado.</p>
                    </div>
                </div>
            </div>

            <div class='section'>
                <h2>🛠️ Stack Tecnológico</h2>
                <div class='tech-stack'>
                    <span class='tech-badge'>.NET Core 2.1</span>
                    <span class='tech-badge'>ASP.NET Core Web API</span>
                    <span class='tech-badge'>Entity Framework Core</span>
                    <span class='tech-badge'>SQL Server</span>
                    <span class='tech-badge'>JWT Authentication</span>
                    <span class='tech-badge'>CORS Enabled</span>
                    <span class='tech-badge'>ClosedXML (Excel)</span>
                    <span class='tech-badge'>iTextSharp (PDF)</span>
                </div>
            </div>

            <div class='section'>
                <h2>🔒 Características de Seguridad</h2>
                <p>
                    ✓ Autenticación basada en tokens JWT<br>
                    ✓ Contraseñas hasheadas con HMACSHA512 + salt<br>
                    ✓ Tokens con expiración de 24 horas<br>
                    ✓ Control de acceso basado en roles<br>
                    ✓ Validación de datos en todas las operaciones
                </p>
            </div>

            <div class='status'>
                <span class='status-badge'>✓ API Funcionando Correctamente</span>
                <p style='margin-top: 15px; color: #155724;'>
                    El servidor está activo y listo para recibir peticiones
                </p>
            </div>
        </div>

        <div class='footer'>
            <p>© 2026 Instituto Teológico Vino Nuevo JBP | API REST v1.0</p>
            <p style='margin-top: 10px; font-size: 0.9em;'>
                Powered by .NET Core 2.1 | Desarrollado por Kevin Vanegas
            </p>
        </div>
    </div>
</body>
</html>";

            return new ContentResult
            {
                ContentType = "text/html",
                Content = html
            };
        }
    }
}
