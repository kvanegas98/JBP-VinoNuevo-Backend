# Cambios en Sistema de Precios - Cursos Especializados

## Fecha
2026-01-19

## Descripción del Cambio
Se modificó el sistema de precios de cursos especializados para que funcione **exactamente igual** que el sistema académico normal. Los precios ahora son **globales** (configuración por CategoríaEstudiante + Cargo), no específicos por curso.

### Antes (❌ Removido)
- Precios por combinación: `CursoEspecializadoId` + `ModalidadId` + `CategoriaEstudianteId`
- Cada curso tenía su propia configuración de precios
- Campo: `Monto`

### Ahora (✅ Implementado)
- Precios globales por combinación: `CategoriaEstudianteId` + `CargoId` (nullable)
- Misma lógica que PrecioMatricula y PrecioMensualidad del sistema académico
- Campo: `Precio`

---

## Archivos Modificados

### 1. Entidades

#### `Sistema.Entidades/Catalogos/PrecioMatriculaCurso.cs`
```csharp
// ANTES
public int CursoEspecializadoId { get; set; }
public int ModalidadId { get; set; }
public int CategoriaEstudianteId { get; set; }
public decimal Monto { get; set; }

// AHORA
public int CategoriaEstudianteId { get; set; }
public int? CargoId { get; set; }  // NULL = Externos, ID = Internos
public decimal Precio { get; set; }
```

#### `Sistema.Entidades/Catalogos/PrecioMensualidadCurso.cs`
```csharp
// ANTES
public int CursoEspecializadoId { get; set; }
public int ModalidadId { get; set; }
public int CategoriaEstudianteId { get; set; }
public decimal Monto { get; set; }

// AHORA
public int CategoriaEstudianteId { get; set; }
public int? CargoId { get; set; }  // NULL = Externos, ID = Internos
public decimal Precio { get; set; }
```

#### `Sistema.Entidades/Catalogos/CursoEspecializado.cs`
```csharp
// ELIMINADAS estas navegaciones:
public ICollection<PrecioMatriculaCurso> PreciosMatricula { get; set; }
public ICollection<PrecioMensualidadCurso> PreciosMensualidad { get; set; }

// MANTIENE SOLO:
public ICollection<Instituto.MatriculaCurso> Matriculas { get; set; }
```

---

### 2. Controllers Nuevos

#### `PreciosMatriculaCursoController.cs` (Nuevo)
Reemplaza los endpoints de configuración de precios que estaban en `CursosEspecializadosController`.

**Endpoints:**
- `GET /api/PreciosMatriculaCurso/Listar`
- `GET /api/PreciosMatriculaCurso/Obtener/{id}`
- `GET /api/PreciosMatriculaCurso/ObtenerPrecio/{categoriaId}/{cargoId?}`
- `POST /api/PreciosMatriculaCurso/Crear`
- `PUT /api/PreciosMatriculaCurso/Actualizar`
- `DELETE /api/PreciosMatriculaCurso/Eliminar/{id}`

#### `PreciosMensualidadCursoController.cs` (Nuevo)
**Endpoints:**
- `GET /api/PreciosMensualidadCurso/Listar`
- `GET /api/PreciosMensualidadCurso/Obtener/{id}`
- `GET /api/PreciosMensualidadCurso/ObtenerPrecio/{categoriaId}/{cargoId?}`
- `POST /api/PreciosMensualidadCurso/Crear`
- `PUT /api/PreciosMensualidadCurso/Actualizar`
- `DELETE /api/PreciosMensualidadCurso/Eliminar/{id}`

---

### 3. Controllers Modificados

#### `CursosEspecializadosController.cs`

**ELIMINADOS estos endpoints:**
- ❌ `POST /api/CursosEspecializados/ConfigurarPrecioMatricula`
- ❌ `POST /api/CursosEspecializados/ConfigurarPrecioMensualidad`

**MODIFICADOS:**
- `POST /api/CursosEspecializados/Crear`
  - Ahora usa ViewModel `CrearCursoEspecializadoViewModel`
  - NO enviar `cursoEspecializadoId` en el body

- `PUT /api/CursosEspecializados/Actualizar/{id}`
  - Ahora el ID va en la URL (route parameter)
  - Usa ViewModel `ActualizarCursoEspecializadoViewModel`
  - NO enviar `cursoEspecializadoId` en el body

- `GET /api/CursosEspecializados/Obtener/{id}`
  - Ya NO retorna precios (PreciosMatricula, PreciosMensualidad eliminados del response)

#### `MatriculasCursoController.cs`

**Cambio en lógica de precios:**
```csharp
// ANTES
var precio = await _context.PreciosMatriculaCurso
    .FirstOrDefaultAsync(p =>
        p.CursoEspecializadoId == model.CursoEspecializadoId &&
        p.ModalidadId == model.ModalidadId &&
        p.CategoriaEstudianteId == model.CategoriaEstudianteId &&
        p.Activo);

// AHORA
// 1. Obtener cargo del estudiante si es interno
int? cargoId = null;
if (estudiante.EsInterno && estudiante.EstudianteCargos != null)
{
    var cargo = estudiante.EstudianteCargos.First(ec => ec.Cargo != null);
    cargoId = cargo.CargoId;
}

// 2. Buscar precio por categoría + cargo
var precio = await _context.PreciosMatriculaCurso
    .FirstOrDefaultAsync(p =>
        p.CategoriaEstudianteId == model.CategoriaEstudianteId &&
        p.CargoId == cargoId &&
        p.Activo);

// 3. Fallback a precio sin cargo si no encuentra
if (precio == null && cargoId.HasValue)
{
    precio = await _context.PreciosMatriculaCurso
        .FirstOrDefaultAsync(p =>
            p.CategoriaEstudianteId == model.CategoriaEstudianteId &&
            p.CargoId == null &&
            p.Activo);
}
```

#### `PagosCursoController.cs`

**Mismos cambios que MatriculasCursoController** - Ahora busca precios por CategoríaEstudiante + Cargo.

---

## Cambios para el Frontend

### 1. Gestión de Precios

#### ❌ ELIMINAR estas pantallas/componentes:
- Configuración de precios desde la pantalla de cursos
- Endpoints `ConfigurarPrecioMatricula` y `ConfigurarPrecioMensualidad` del módulo de cursos

#### ✅ CREAR nuevas pantallas (Duplicar de sistema académico):
Crear pantallas separadas para gestión de precios globales:

**a) Pantalla: "Precios Matrícula - Cursos Especializados"**
- Ruta: `/configuracion/precios-matricula-curso` o similar
- API: `/api/PreciosMatriculaCurso`
- Igual que la pantalla de "Precios Matrícula" del sistema académico
- Campos:
  - Categoría Estudiante (select)
  - Cargo (select, opcional - NULL para externos)
  - Precio (decimal)
  - Activo (checkbox)

**b) Pantalla: "Precios Mensualidad - Cursos Especializados"**
- Ruta: `/configuracion/precios-mensualidad-curso` o similar
- API: `/api/PreciosMensualidadCurso`
- Igual que la pantalla de "Precios Mensualidad" del sistema académico
- Campos:
  - Categoría Estudiante (select)
  - Cargo (select, opcional - NULL para externos)
  - Precio (decimal)
  - Activo (checkbox)

---

### 2. Crear Curso Especializado

#### Antes (❌):
```javascript
POST /api/CursosEspecializados/Crear
{
  "cursoEspecializadoId": null,  // ❌ Causaba error
  "nombre": "Excel Avanzado",
  "descripcion": "Descripción",
  "fechaInicio": "2026-02-01",
  "fechaFin": "2026-05-01"
}
```

#### Ahora (✅):
```javascript
POST /api/CursosEspecializados/Crear
{
  "nombre": "Excel Avanzado",
  "descripcion": "Descripción",
  "fechaInicio": "2026-02-01",
  "fechaFin": "2026-05-01"
}
```

**Response:**
```javascript
{
  "cursoEspecializadoId": 1,
  "codigo": "CURSO-2026-001",
  "nombre": "Excel Avanzado",
  "descripcion": "Descripción",
  "fechaInicio": "2026-02-01",
  "fechaFin": "2026-05-01",
  "activo": true,
  "fechaCreacion": "2026-01-19T10:30:00"
}
```

---

### 3. Actualizar Curso Especializado

#### Antes (❌):
```javascript
PUT /api/CursosEspecializados/Actualizar
{
  "cursoEspecializadoId": 5,
  "nombre": "Excel Avanzado",
  "descripcion": "Descripción",
  "fechaInicio": "2026-02-01",
  "fechaFin": "2026-05-01"
}
```

#### Ahora (✅):
```javascript
PUT /api/CursosEspecializados/Actualizar/5
{
  "nombre": "Excel Avanzado",
  "descripcion": "Descripción actualizada",
  "fechaInicio": "2026-02-01",
  "fechaFin": "2026-05-01",
  "activo": true
}
```

---

### 4. Obtener Curso Especializado

#### Response Antes (❌):
```javascript
{
  "cursoEspecializadoId": 1,
  "codigo": "CURSO-2026-001",
  "nombre": "Excel Avanzado",
  // ...
  "preciosMatricula": [...],      // ❌ Eliminado
  "preciosMensualidad": [...]     // ❌ Eliminado
}
```

#### Response Ahora (✅):
```javascript
{
  "cursoEspecializadoId": 1,
  "codigo": "CURSO-2026-001",
  "nombre": "Excel Avanzado",
  "descripcion": "Descripción",
  "fechaInicio": "2026-02-01",
  "fechaFin": "2026-05-01",
  "activo": true,
  "fechaCreacion": "2026-01-19T10:30:00"
}
// Los precios se consultan desde PreciosMatriculaCurso y PreciosMensualidadCurso
```

---

### 5. Configurar Precios (Nuevos Endpoints)

#### Crear Precio de Matrícula:
```javascript
POST /api/PreciosMatriculaCurso/Crear
{
  "categoriaEstudianteId": 1,
  "cargoId": null,         // NULL = Externos, ID = Internos
  "precio": 50.00,
  "activo": true
}
```

#### Crear Precio de Mensualidad:
```javascript
POST /api/PreciosMensualidadCurso/Crear
{
  "categoriaEstudianteId": 1,
  "cargoId": 2,            // ID del cargo específico
  "precio": 15.00,
  "activo": true
}
```

#### Listar Precios:
```javascript
GET /api/PreciosMatriculaCurso/Listar
GET /api/PreciosMensualidadCurso/Listar
```

**Response:**
```javascript
[
  {
    "precioMatriculaCursoId": 1,
    "categoriaEstudianteId": 1,
    "categoriaEstudianteNombre": "Regular",
    "cargoId": null,
    "cargoNombre": "Sin cargo (Externo)",
    "precio": 50.00,
    "activo": true
  },
  {
    "precioMatriculaCursoId": 2,
    "categoriaEstudianteId": 1,
    "categoriaEstudianteNombre": "Regular",
    "cargoId": 3,
    "cargoNombre": "Pastor",
    "precio": 30.00,
    "activo": true
  }
]
```

---

## Flujo Completo de Configuración

### 1. Configurar Precios Globales (Una sola vez)
```javascript
// Configurar precio de matrícula para externos regulares
POST /api/PreciosMatriculaCurso/Crear
{
  "categoriaEstudianteId": 1,  // Regular
  "cargoId": null,              // Externo
  "precio": 50.00,
  "activo": true
}

// Configurar precio de matrícula para internos pastores
POST /api/PreciosMatriculaCurso/Crear
{
  "categoriaEstudianteId": 1,  // Regular
  "cargoId": 3,                 // Pastor
  "precio": 30.00,
  "activo": true
}

// Configurar precio de mensualidad para externos regulares
POST /api/PreciosMensualidadCurso/Crear
{
  "categoriaEstudianteId": 1,  // Regular
  "cargoId": null,              // Externo
  "precio": 15.00,
  "activo": true
}
```

### 2. Crear Cursos (Sin configurar precios)
```javascript
POST /api/CursosEspecializados/Crear
{
  "nombre": "Excel Avanzado",
  "descripcion": "Curso de Excel",
  "fechaInicio": "2026-02-01",
  "fechaFin": "2026-05-01"
}
```

### 3. Matricular Estudiante
El sistema automáticamente:
1. Detecta si el estudiante es interno o externo
2. Obtiene su cargo (si es interno)
3. Busca el precio configurado para su categoría + cargo
4. Aplica becas si corresponde

---

## Script de Migración de Base de Datos

**Ubicación:** `scripts/actualizar_precios_cursos_especializados.sql`

**Ejecutar:**
```sql
-- Ejecutar el script completo en SQL Server Management Studio
-- El script está en una transacción, si falla se revierte todo
```

**Cambios que realiza:**
1. Elimina columnas: `CursoEspecializadoId`, `ModalidadId` de ambas tablas
2. Renombra `Monto` → `Precio` en ambas tablas
3. Agrega columna `CargoId` (nullable) en ambas tablas
4. Crea foreign keys e índices necesarios

---

## Resumen de Beneficios

### Antes
- ❌ Precios configurados por curso
- ❌ Duplicación de configuración para cada curso
- ❌ Difícil mantenimiento
- ❌ Lógica diferente al sistema académico

### Ahora
- ✅ Precios globales (una configuración sirve para todos los cursos)
- ✅ Lógica unificada con sistema académico
- ✅ Más simple de mantener
- ✅ Menos datos duplicados
- ✅ Mismo comportamiento que PrecioMatricula y PrecioMensualidad

---

## Notas Importantes

1. **Los precios ahora son globales**: No necesitas configurar precios para cada curso, solo una vez por combinación de categoría + cargo.

2. **Endpoints eliminados**: `ConfigurarPrecioMatricula` y `ConfigurarPrecioMensualidad` del controller de cursos ya no existen.

3. **ViewModels nuevos**: `CrearCursoEspecializadoViewModel` y `ActualizarCursoEspecializadoViewModel` - no incluyen el ID.

4. **Campo renombrado**: `Monto` → `Precio` (igual que sistema académico).

5. **Cargo nullable**: `CargoId` puede ser NULL (estudiantes externos), o tener un ID (estudiantes internos con cargo específico).
