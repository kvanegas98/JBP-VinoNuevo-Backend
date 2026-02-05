# Documentación API - Módulo de Cursos Especializados

## Descripción General

Esta documentación describe todos los endpoints del módulo de Cursos Especializados para su implementación en el frontend Vue.js.

**IMPORTANTE**: Los cursos especializados son simples - solo constan del curso en sí, sin materias/asignaturas. Los pagos son:
- Matrícula (pago inicial)
- Mensualidades numeradas (1, 2, 3, etc.) calculadas automáticamente según la duración del curso

**Base URL**: `https://tu-api.com/api`

**Headers requeridos en todas las peticiones**:
```
Content-Type: application/json
Authorization: Bearer {token}
```

---

## 1. GESTIÓN DE CURSOS ESPECIALIZADOS

### 1.1 Listar Cursos Especializados

**Endpoint**: `GET /CursosEspecializados/Listar`

**Query Parameters**:
- `soloActivos` (boolean, opcional): Si es `true`, solo devuelve cursos activos

**Response 200 OK**:
```json
[
  {
    "cursoEspecializadoId": 1,
    "codigo": "CURSO-2026-001",
    "nombre": "Liderazgo Pastoral Avanzado",
    "descripcion": "Curso intensivo de liderazgo para pastores",
    "fechaInicio": "2026-02-01T00:00:00",
    "fechaFin": "2026-06-30T00:00:00",
    "activo": true,
    "fechaCreacion": "2026-01-15T10:30:00",
    "totalMatriculas": 15
  }
]
```

**Errores posibles**:
- 401 Unauthorized: Token inválido o expirado
- 500 Internal Server Error: Error en el servidor

---

### 1.2 Obtener Curso Especializado por ID

**Endpoint**: `GET /CursosEspecializados/Obtener/{id}`

**Path Parameters**:
- `id` (int, requerido): ID del curso

**Response 200 OK**:
```json
{
  "cursoEspecializadoId": 1,
  "codigo": "CURSO-2026-001",
  "nombre": "Liderazgo Pastoral Avanzado",
  "descripcion": "Curso intensivo de liderazgo para pastores",
  "fechaInicio": "2026-02-01T00:00:00",
  "fechaFin": "2026-06-30T00:00:00",
  "activo": true,
  "fechaCreacion": "2026-01-15T10:30:00",
  "preciosMatricula": [
    {
      "precioMatriculaCursoId": 1,
      "modalidadId": 1,
      "modalidadNombre": "Presencial",
      "categoriaEstudianteId": 1,
      "categoriaEstudianteNombre": "Regular",
      "monto": 50.00,
      "activo": true
    }
  ],
  "preciosMensualidad": [
    {
      "precioMensualidadCursoId": 1,
      "modalidadId": 1,
      "modalidadNombre": "Presencial",
      "categoriaEstudianteId": 1,
      "categoriaEstudianteNombre": "Regular",
      "monto": 25.00,
      "activo": true
    }
  ]
}
```

**Nota**: La duración del curso se calcula automáticamente de `fechaInicio` a `fechaFin`. Ejemplo: curso de Feb-Jun = 5 meses = 5 mensualidades.

**Errores posibles**:
- 404 Not Found: Curso no encontrado
- 401 Unauthorized: Token inválido
- 500 Internal Server Error: Error en el servidor

---

### 1.3 Obtener Cursos para Select

**Endpoint**: `GET /CursosEspecializados/Select`

**Descripción**: Devuelve lista simplificada de cursos activos y vigentes (FechaFin >= Hoy) para usar en dropdowns

**Response 200 OK**:
```json
[
  {
    "id": 1,
    "nombre": "Liderazgo Pastoral Avanzado"
  },
  {
    "id": 2,
    "nombre": "Teología Sistemática"
  }
]
```

**Errores posibles**:
- 401 Unauthorized: Token inválido
- 500 Internal Server Error: Error en el servidor

---

### 1.4 Crear Curso Especializado

**Endpoint**: `POST /CursosEspecializados/Crear`

**Request Body**:
```json
{
  "nombre": "Liderazgo Pastoral Avanzado",
  "descripcion": "Curso intensivo de liderazgo para pastores y líderes de la iglesia",
  "fechaInicio": "2026-02-01",
  "fechaFin": "2026-06-30"
}
```

**Nota**: El campo `codigo` se genera automáticamente con el formato `CURSO-{año}-{secuencial}` (ej: CURSO-2026-001)

**Response 200 OK**:
```json
{
  "cursoEspecializadoId": 1,
  "codigo": "CURSO-2026-001",
  "nombre": "Liderazgo Pastoral Avanzado",
  "descripcion": "Curso intensivo de liderazgo para pastores y líderes de la iglesia",
  "fechaInicio": "2026-02-01T00:00:00",
  "fechaFin": "2026-06-30T00:00:00",
  "activo": true,
  "fechaCreacion": "2026-01-17T14:25:00"
}
```

**Errores posibles**:
- 400 Bad Request:
  - Datos inválidos (campos requeridos faltantes)
  - "La fecha de inicio debe ser anterior a la fecha de fin"
- 401 Unauthorized: Token inválido
- 500 Internal Server Error: Error en el servidor

---

### 1.5 Actualizar Curso Especializado

**Endpoint**: `PUT /CursosEspecializados/Actualizar`

**Request Body**:
```json
{
  "cursoEspecializadoId": 1,
  "nombre": "Liderazgo Pastoral Avanzado - Actualizado",
  "descripcion": "Curso intensivo y práctico de liderazgo",
  "fechaInicio": "2026-02-01",
  "fechaFin": "2026-07-31"
}
```

**Nota**: No se puede cambiar el código. Solo se actualizan: nombre, descripción, y fechas.

**Response 200 OK**:
```json
{
  "cursoEspecializadoId": 1,
  "codigo": "CURSO-2026-001",
  "nombre": "Liderazgo Pastoral Avanzado - Actualizado",
  "descripcion": "Curso intensivo y práctico de liderazgo",
  "fechaInicio": "2026-02-01T00:00:00",
  "fechaFin": "2026-07-31T00:00:00",
  "activo": true,
  "fechaCreacion": "2026-01-15T10:30:00"
}
```

**Errores posibles**:
- 400 Bad Request:
  - Datos inválidos
  - "La fecha de inicio debe ser anterior a la fecha de fin"
- 404 Not Found: Curso no encontrado
- 401 Unauthorized: Token inválido
- 500 Internal Server Error: Error en el servidor

---

### 1.6 Activar Curso Especializado

**Endpoint**: `PUT /CursosEspecializados/Activar/{id}`

**Path Parameters**:
- `id` (int, requerido): ID del curso

**Response 200 OK**: (sin cuerpo)

**Errores posibles**:
- 404 Not Found: Curso no encontrado
- 401 Unauthorized: Token inválido
- 500 Internal Server Error: Error en el servidor

---

### 1.7 Desactivar Curso Especializado

**Endpoint**: `PUT /CursosEspecializados/Desactivar/{id}`

**Path Parameters**:
- `id` (int, requerido): ID del curso

**Response 200 OK**: (sin cuerpo)

**Errores posibles**:
- 400 Bad Request: "No se puede desactivar un curso con matrículas activas"
- 404 Not Found: Curso no encontrado
- 401 Unauthorized: Token inválido
- 500 Internal Server Error: Error en el servidor

---

### 1.8 Configurar Precio de Matrícula

**Endpoint**: `POST /CursosEspecializados/ConfigurarPrecioMatricula`

**Descripción**: Crea o actualiza el precio de matrícula para una combinación específica de curso + modalidad + categoría estudiante.

**Request Body**:
```json
{
  "cursoEspecializadoId": 1,
  "modalidadId": 1,
  "categoriaEstudianteId": 1,
  "monto": 50.00,
  "activo": true
}
```

**Response 200 OK**: (sin cuerpo)

**Errores posibles**:
- 400 Bad Request: Datos inválidos
- 401 Unauthorized: Token inválido
- 500 Internal Server Error: Error en el servidor

---

### 1.9 Configurar Precio de Mensualidad

**Endpoint**: `POST /CursosEspecializados/ConfigurarPrecioMensualidad`

**Descripción**: Crea o actualiza el precio de mensualidad para una combinación específica de curso + modalidad + categoría estudiante. Este precio se aplica a todas las mensualidades del curso.

**Request Body**:
```json
{
  "cursoEspecializadoId": 1,
  "modalidadId": 1,
  "categoriaEstudianteId": 1,
  "monto": 25.00,
  "activo": true
}
```

**Response 200 OK**: (sin cuerpo)

**Errores posibles**:
- 400 Bad Request: Datos inválidos
- 401 Unauthorized: Token inválido
- 500 Internal Server Error: Error en el servidor

---

## 2. GESTIÓN DE MATRÍCULAS DE CURSOS

### 2.1 Listar Matrículas de Cursos

**Endpoint**: `GET /MatriculasCurso/Listar`

**Query Parameters**:
- `cursoEspecializadoId` (int, opcional): Filtrar por curso específico
- `estado` (string, opcional): Filtrar por estado (Pendiente, Activa, Completada, Anulada)

**Response 200 OK**:
```json
[
  {
    "matriculaCursoId": 1,
    "codigo": "MCURSO-2026-0001",
    "estudianteId": 5,
    "estudianteCodigo": "EST-2024-001",
    "estudianteNombre": "Juan Pérez",
    "cursoEspecializadoId": 1,
    "cursoNombre": "Liderazgo Pastoral Avanzado",
    "modalidadId": 1,
    "modalidadNombre": "Presencial",
    "categoriaEstudianteId": 1,
    "categoriaNombre": "Regular",
    "fechaMatricula": "2026-01-20T10:00:00",
    "montoMatricula": 50.00,
    "descuentoAplicado": 25.00,
    "montoFinal": 25.00,
    "estado": "Activa",
    "aprobado": false
  }
]
```

**Errores posibles**:
- 401 Unauthorized: Token inválido
- 500 Internal Server Error: Error en el servidor

---

### 2.2 Obtener Matrícula de Curso por ID

**Endpoint**: `GET /MatriculasCurso/Obtener/{id}`

**Path Parameters**:
- `id` (int, requerido): ID de la matrícula de curso

**Response 200 OK**:
```json
{
  "matriculaCursoId": 1,
  "codigo": "MCURSO-2026-0001",
  "estudianteId": 5,
  "estudianteCodigo": "EST-2024-001",
  "estudianteNombre": "Juan Pérez",
  "estudianteTelefono": "88887777",
  "cursoEspecializadoId": 1,
  "cursoNombre": "Liderazgo Pastoral Avanzado",
  "cursoCodigo": "CURSO-2026-001",
  "modalidadId": 1,
  "modalidadNombre": "Presencial",
  "categoriaEstudianteId": 1,
  "categoriaNombre": "Regular",
  "fechaMatricula": "2026-01-20T10:00:00",
  "montoMatricula": 50.00,
  "descuentoAplicado": 25.00,
  "montoFinal": 25.00,
  "estado": "Activa",
  "aprobado": false,
  "esBecado": true,
  "porcentajeBeca": 50.00
}
```

**Errores posibles**:
- 404 Not Found: Matrícula de curso no encontrada
- 401 Unauthorized: Token inválido
- 500 Internal Server Error: Error en el servidor

---

### 2.3 Crear Matrícula de Curso

**Endpoint**: `POST /MatriculasCurso/Crear`

**Request Body**:
```json
{
  "estudianteId": 5,
  "cursoEspecializadoId": 1,
  "modalidadId": 1,
  "categoriaEstudianteId": 1
}
```

**Lógica de negocio automática**:
1. Genera código `MCURSO-{año}-{secuencial}`
2. Busca precio de matrícula según curso + modalidad + categoría
3. Si el estudiante es becado, aplica descuento automático
4. Calcula `montoFinal = montoMatricula - descuentoAplicado`
5. Si `montoFinal == 0` (becado 100%), activa matrícula automáticamente
6. Si `montoFinal > 0`, la matrícula queda en estado "Pendiente" hasta el pago

**Response 200 OK**:
```json
{
  "matriculaCursoId": 1,
  "codigo": "MCURSO-2026-0001",
  "estudianteId": 5,
  "estudianteNombre": "Juan Pérez",
  "cursoEspecializadoId": 1,
  "cursoNombre": "Liderazgo Pastoral Avanzado",
  "modalidadId": 1,
  "modalidadNombre": "Presencial",
  "categoriaEstudianteId": 1,
  "categoriaNombre": "Regular",
  "fechaMatricula": "2026-01-20T10:00:00",
  "montoMatricula": 50.00,
  "descuentoAplicado": 25.00,
  "montoFinal": 25.00,
  "estado": "Pendiente",
  "aprobado": false,
  "esBecado": true,
  "porcentajeBeca": 50.00,
  "message": "Matrícula creada. Descuento de beca aplicado: 50%"
}
```

**Errores posibles**:
- 400 Bad Request:
  - Datos inválidos (campos requeridos faltantes)
  - "El estudiante no existe"
  - "El curso no existe o no está activo"
  - "La modalidad no existe"
  - "La categoría de estudiante no existe"
  - "El estudiante ya está matriculado en este curso"
  - "El estudiante ya tiene una matrícula aprobada en este curso y no puede volver a inscribirse"
  - "No se encontró precio de matrícula configurado para esta modalidad y categoría"
- 401 Unauthorized: Token inválido
- 500 Internal Server Error: Error en el servidor

---

### 2.4 Anular Matrícula de Curso

**Endpoint**: `PUT /MatriculasCurso/Anular/{id}`

**Path Parameters**:
- `id` (int, requerido): ID de la matrícula de curso

**Response 200 OK**: (sin cuerpo)

**Errores posibles**:
- 400 Bad Request: "No se puede anular una matrícula con pagos de mensualidad registrados"
- 404 Not Found: Matrícula de curso no encontrada
- 401 Unauthorized: Token inválido
- 500 Internal Server Error: Error en el servidor

---

### 2.5 Marcar Curso como Aprobado

**Endpoint**: `PUT /MatriculasCurso/MarcarAprobado/{id}`

**Path Parameters**:
- `id` (int, requerido): ID de la matrícula de curso

**Response 200 OK**: (sin cuerpo)

**Lógica de negocio**:
- Marca `Aprobado = true`
- Cambia `Estado = "Completada"`
- Una vez aprobado, el estudiante no puede volver a matricularse en el mismo curso

**Errores posibles**:
- 400 Bad Request: "La matrícula debe estar activa para marcarla como aprobada"
- 404 Not Found: Matrícula de curso no encontrada
- 401 Unauthorized: Token inválido
- 500 Internal Server Error: Error en el servidor

---

## 3. GESTIÓN DE PAGOS DE CURSOS

### 3.1 Listar Pagos de Cursos

**Endpoint**: `GET /PagosCurso/Listar`

**Query Parameters**:
- `matriculaCursoId` (int, opcional): Filtrar por matrícula específica

**Response 200 OK**:
```json
[
  {
    "pagoCursoId": 1,
    "codigo": "PCURSO-2026-0001",
    "matriculaCursoId": 1,
    "matriculaCursoCodigo": "MCURSO-2026-0001",
    "estudianteId": 5,
    "estudianteCodigo": "EST-2024-001",
    "estudianteNombre": "Juan Pérez",
    "numeroMensualidad": null,
    "cursoEspecializadoId": 1,
    "cursoNombre": "Liderazgo Pastoral Avanzado",
    "tipoPago": "Matricula",
    "monto": 50.00,
    "descuento": 25.00,
    "montoFinal": 25.00,
    "fechaPago": "2026-01-20T10:15:00",
    "efectivoCordobas": 0,
    "efectivoDolares": 25.00,
    "tarjetaCordobas": 0,
    "tarjetaDolares": 0,
    "tipoCambio": 36.50,
    "totalPagadoUSD": 25.00,
    "metodoPago": "Efectivo",
    "numeroComprobante": null,
    "observaciones": null,
    "estado": "Completado"
  },
  {
    "pagoCursoId": 2,
    "codigo": "PCURSO-2026-0002",
    "matriculaCursoId": 1,
    "matriculaCursoCodigo": "MCURSO-2026-0001",
    "estudianteId": 5,
    "estudianteCodigo": "EST-2024-001",
    "estudianteNombre": "Juan Pérez",
    "numeroMensualidad": 1,
    "cursoEspecializadoId": 1,
    "cursoNombre": "Liderazgo Pastoral Avanzado",
    "tipoPago": "Mensualidad",
    "monto": 25.00,
    "descuento": 12.50,
    "montoFinal": 12.50,
    "fechaPago": "2026-02-05T09:30:00",
    "efectivoCordobas": 456.25,
    "efectivoDolares": 0,
    "tarjetaCordobas": 0,
    "tarjetaDolares": 0,
    "tipoCambio": 36.50,
    "totalPagadoUSD": 12.50,
    "metodoPago": "Efectivo",
    "numeroComprobante": null,
    "observaciones": null,
    "estado": "Completado"
  }
]
```

**Nota**: `numeroMensualidad` es `null` para pagos de matrícula, y un número (1, 2, 3, etc.) para pagos de mensualidades.

**Errores posibles**:
- 401 Unauthorized: Token inválido
- 500 Internal Server Error: Error en el servidor

---

### 3.2 Obtener Matrículas de Curso Pendientes

**Endpoint**: `GET /PagosCurso/MatriculasCursoPendientes/{estudianteId}`

**Descripción**: Devuelve matrículas en estado "Pendiente" (esperando pago de matrícula)

**Path Parameters**:
- `estudianteId` (int, requerido): ID del estudiante

**Response 200 OK**:
```json
[
  {
    "matriculaCursoId": 1,
    "codigo": "MCURSO-2026-0001",
    "cursoEspecializadoId": 1,
    "cursoNombre": "Liderazgo Pastoral Avanzado",
    "modalidadNombre": "Presencial",
    "categoriaNombre": "Regular",
    "fechaMatricula": "2026-01-20T10:00:00",
    "montoMatricula": 50.00,
    "descuentoAplicado": 25.00,
    "montoFinal": 25.00,
    "estado": "Pendiente"
  }
]
```

**Errores posibles**:
- 401 Unauthorized: Token inválido
- 500 Internal Server Error: Error en el servidor

---

### 3.3 Obtener Matrículas de Curso Activas

**Endpoint**: `GET /PagosCurso/MatriculasCursoActivas/{estudianteId}`

**Descripción**: Devuelve matrículas en estado "Activa" (ya pagaron matrícula, pueden pagar mensualidades)

**Path Parameters**:
- `estudianteId` (int, requerido): ID del estudiante

**Response 200 OK**:
```json
[
  {
    "matriculaCursoId": 1,
    "codigo": "MCURSO-2026-0001",
    "cursoEspecializadoId": 1,
    "cursoNombre": "Liderazgo Pastoral Avanzado",
    "modalidadNombre": "Presencial",
    "categoriaNombre": "Regular",
    "fechaMatricula": "2026-01-20T10:00:00",
    "montoMatricula": 50.00,
    "descuentoAplicado": 25.00,
    "montoFinal": 25.00,
    "estado": "Activa"
  }
]
```

**Errores posibles**:
- 401 Unauthorized: Token inválido
- 500 Internal Server Error: Error en el servidor

---

### 3.4 Obtener Mensualidades para Pago

**Endpoint**: `GET /PagosCurso/MensualidadesParaPago/{matriculaCursoId}`

**Descripción**: Obtiene la lista de mensualidades del curso (calculadas automáticamente según duración del curso), indicando cuáles están pagadas y cuáles pendientes.

**Path Parameters**:
- `matriculaCursoId` (int, requerido): ID de la matrícula de curso

**Response 200 OK**:
```json
{
  "matriculaCursoId": 1,
  "matriculaCursoCodigo": "MCURSO-2026-0001",
  "estudianteNombre": "Juan Pérez",
  "cursoNombre": "Liderazgo Pastoral Avanzado",
  "categoriaNombre": "Regular",
  "esBecado": true,
  "porcentajeBeca": 50.00,
  "esBecado100": false,
  "estadoMatricula": "Activa",
  "soloLectura": false,
  "mensualidades": [
    {
      "numeroMensualidad": 1,
      "nombre": "Mensualidad 1",
      "pagado": true,
      "pagadoAutomaticamente": false,
      "montoBase": 25.00,
      "descuento": 12.50,
      "montoFinal": 12.50
    },
    {
      "numeroMensualidad": 2,
      "nombre": "Mensualidad 2",
      "pagado": false,
      "pagadoAutomaticamente": false,
      "montoBase": 25.00,
      "descuento": 12.50,
      "montoFinal": 12.50
    },
    {
      "numeroMensualidad": 3,
      "nombre": "Mensualidad 3",
      "pagado": false,
      "pagadoAutomaticamente": false,
      "montoBase": 25.00,
      "descuento": 12.50,
      "montoFinal": 12.50
    },
    {
      "numeroMensualidad": 4,
      "nombre": "Mensualidad 4",
      "pagado": false,
      "pagadoAutomaticamente": false,
      "montoBase": 25.00,
      "descuento": 12.50,
      "montoFinal": 12.50
    },
    {
      "numeroMensualidad": 5,
      "nombre": "Mensualidad 5",
      "pagado": false,
      "pagadoAutomaticamente": false,
      "montoBase": 25.00,
      "descuento": 12.50,
      "montoFinal": 12.50
    }
  ],
  "resumen": {
    "totalMensualidades": 5,
    "mensualidadesPagadas": 1,
    "mensualidadesPendientes": 4,
    "montoPendiente": 50.00
  }
}
```

**Nota importante sobre becas**:
- Si el estudiante es becado al 100% (`esBecado100 = true`), todas las mensualidades aparecen como `pagado = true` y `pagadoAutomaticamente = true`
- No se requiere registrar pagos para becados al 100%
- El descuento de beca se aplica automáticamente a todas las mensualidades

**Nota importante sobre duración**:
- El número de mensualidades se calcula automáticamente desde `FechaInicio` hasta `FechaFin` del curso
- Ejemplo: Curso del 1 Feb al 30 Jun = 5 meses = 5 mensualidades (1, 2, 3, 4, 5)
- No hay nombres de materias, solo números de mensualidad

**Errores posibles**:
- 400 Bad Request: "La matrícula debe estar activa o completada para consultar mensualidades"
- 404 Not Found: Matrícula de curso no encontrada
- 401 Unauthorized: Token inválido
- 500 Internal Server Error: Error en el servidor

---

### 3.5 Pagar Matrícula de Curso

**Endpoint**: `POST /PagosCurso/PagarMatriculaCurso`

**Descripción**: Registra el pago de matrícula y activa la matrícula del curso

**Request Body**:
```json
{
  "matriculaCursoId": 1,
  "observaciones": "Pago en efectivo",
  "detallePago": {
    "efectivoCordobas": 912.50,
    "efectivoDolares": 0,
    "tarjetaCordobas": 0,
    "tarjetaDolares": 0,
    "tipoCambio": 36.50,
    "vueltoCordobas": 0,
    "vueltoDolares": 0,
    "numeroComprobante": null
  }
}
```

**Descripción de campos de DetallePago**:
- `efectivoCordobas`: Monto pagado en efectivo en córdobas
- `efectivoDolares`: Monto pagado en efectivo en dólares
- `tarjetaCordobas`: Monto pagado con tarjeta en córdobas
- `tarjetaDolares`: Monto pagado con tarjeta en dólares
- `tipoCambio`: Tipo de cambio del día (requerido)
- `vueltoCordobas`: Vuelto entregado en córdobas (calculado en frontend)
- `vueltoDolares`: Vuelto entregado en dólares (calculado en frontend)
- `numeroComprobante`: Número de comprobante/autorización de tarjeta (opcional)

**Lógica del sistema**:
1. Convierte todos los pagos a USD usando el tipo de cambio
2. Valida que el total pagado sea >= al monto final de la matrícula
3. Calcula el vuelto automáticamente
4. Genera código de pago único `PCURSO-{año}-{secuencial}`
5. Activa la matrícula (cambia estado de "Pendiente" a "Activa")

**Response 200 OK**:
```json
{
  "pagoCursoId": 1,
  "codigo": "PCURSO-2026-0001",
  "matriculaCursoId": 1,
  "matriculaCursoCodigo": "MCURSO-2026-0001",
  "estudianteNombre": "Juan Pérez",
  "cursoNombre": "Liderazgo Pastoral Avanzado",
  "montoFinal": 25.00,
  "detallePago": {
    "efectivoCordobas": 912.50,
    "efectivoDolares": 0,
    "tarjetaCordobas": 0,
    "tarjetaDolares": 0,
    "tipoCambio": 36.50,
    "totalPagadoUSD": 25.00
  },
  "vuelto": {
    "totalUSD": 0,
    "cordobas": 0,
    "dolares": 0
  },
  "metodoPago": "Efectivo",
  "estadoMatricula": "Activa",
  "message": "Pago de matrícula registrado y matrícula activada"
}
```

**Errores posibles**:
- 400 Bad Request:
  - Datos inválidos
  - "La matrícula ya está pagada y activa"
  - "No se puede pagar una matrícula anulada"
  - "Ya existe un pago de matrícula para esta inscripción"
  - "No se requiere pago para estudiantes becados al 100%. La matrícula ya fue activada automáticamente."
  - "Debe proporcionar el detalle del pago"
  - "El tipo de cambio debe ser mayor a 0"
  - "El monto pagado es insuficiente" (con detalles: montoRequerido, montoPagado, diferencia)
- 404 Not Found: Matrícula de curso no encontrada
- 401 Unauthorized: Token inválido
- 500 Internal Server Error: Error en el servidor

---

### 3.6 Pagar Mensualidad de Curso

**Endpoint**: `POST /PagosCurso/PagarMensualidadCurso`

**Descripción**: Registra el pago de una mensualidad específica del curso

**Request Body**:
```json
{
  "matriculaCursoId": 1,
  "numeroMensualidad": 2,
  "observaciones": "Pago mensualidad 2",
  "detallePago": {
    "efectivoCordobas": 0,
    "efectivoDolares": 0,
    "tarjetaCordobas": 0,
    "tarjetaDolares": 12.50,
    "tipoCambio": 36.50,
    "vueltoCordobas": 0,
    "vueltoDolares": 0,
    "numeroComprobante": "AUTH-12345678"
  }
}
```

**Nota**: El `numeroMensualidad` debe ser un número del 1 al total de meses del curso (ej: 1, 2, 3, 4, 5 para un curso de 5 meses).

**Response 200 OK**:
```json
{
  "pagoCursoId": 2,
  "codigo": "PCURSO-2026-0002",
  "matriculaCursoId": 1,
  "estudianteNombre": "Juan Pérez",
  "cursoNombre": "Liderazgo Pastoral Avanzado",
  "numeroMensualidad": 2,
  "montoFinal": 12.50,
  "detallePago": {
    "efectivoCordobas": 0,
    "efectivoDolares": 0,
    "tarjetaCordobas": 0,
    "tarjetaDolares": 12.50,
    "tipoCambio": 36.50,
    "totalPagadoUSD": 12.50
  },
  "vuelto": {
    "totalUSD": 0,
    "cordobas": 0,
    "dolares": 0
  },
  "metodoPago": "Tarjeta",
  "message": "Pago de mensualidad 2 registrado exitosamente"
}
```

**Errores posibles**:
- 400 Bad Request:
  - Datos inválidos
  - "La matrícula debe estar activa para registrar pagos de mensualidad"
  - "El número de mensualidad debe ser mayor a 0"
  - "El número de mensualidad no puede ser mayor a {totalMeses}"
  - "Ya existe un pago para la mensualidad {numero}"
  - "No se encontró precio configurado para las mensualidades de este curso"
  - "No se requiere pago para estudiantes becados al 100%"
  - "Debe proporcionar el detalle del pago"
  - "El tipo de cambio debe ser mayor a 0"
  - "El monto pagado es insuficiente" (con detalles)
- 404 Not Found: Matrícula de curso no encontrada
- 401 Unauthorized: Token inválido
- 500 Internal Server Error: Error en el servidor

---

## 4. REPORTES Y CONSULTAS

### 4.1 Reporte de Morosidad (incluye cursos especializados)

**Endpoint**: `GET /Reportes/Morosidad`

**Query Parameters**:
- `redId` (int, opcional): Filtrar por red
- `cursoId` (int, opcional): Filtrar por curso especializado

**Descripción**: Este endpoint ya fue actualizado para incluir tanto módulos académicos como cursos especializados en el reporte de morosidad.

**Response 200 OK**:
```json
[
  {
    "estudianteId": 5,
    "codigo": "EST-2024-001",
    "nombreCompleto": "Juan Pérez",
    "telefono": "88887777",
    "tipo": "Curso Especializado",
    "nombreCurso": "Liderazgo Pastoral Avanzado",
    "fechaPagoMatricula": "2026-01-20T10:15:00",
    "montoPagado": 37.50,
    "montoAdeudado": 25.00,
    "diasMora": 5,
    "montoMora": 2.50
  },
  {
    "estudianteId": 8,
    "codigo": "EST-2024-005",
    "nombreCompleto": "María González",
    "telefono": "77776666",
    "tipo": "Módulo Académico",
    "nombreModulo": "Teología I",
    "anioLectivo": "2025-2026",
    "fechaPagoMatricula": "2025-10-15T09:30:00",
    "montoPagado": 100.00,
    "montoAdeudado": 50.00,
    "diasMora": 10,
    "montoMora": 5.00
  }
]
```

**Lógica de morosidad**:
- La mora se calcula a partir de la fecha de pago de matrícula
- Días de mora = días transcurridos desde fecha de pago de matrícula
- Monto de mora = se calcula igual que en módulos académicos

**Errores posibles**:
- 401 Unauthorized: Token inválido
- 500 Internal Server Error: Error en el servidor

---

## 5. FLUJO COMPLETO DE USO

### Flujo típico para gestionar un curso especializado:

1. **Crear curso especializado** (POST /CursosEspecializados/Crear)
   - Definir nombre, descripción, fechas de inicio y fin
   - El sistema calcula automáticamente el número de meses/mensualidades

2. **Configurar precios** (POST /CursosEspecializados/ConfigurarPrecioMatricula y ConfigurarPrecioMensualidad)
   - Configurar precio de matrícula para cada combinación de modalidad + categoría
   - Configurar precio de mensualidad para cada combinación de modalidad + categoría
   - Estos precios se aplican a todas las mensualidades del curso

3. **Matricular estudiante** (POST /MatriculasCurso/Crear)
   - Seleccionar estudiante, curso, modalidad y categoría
   - El sistema aplica automáticamente descuento de beca si aplica
   - Si es becado 100%, la matrícula se activa automáticamente
   - Si no, queda en estado "Pendiente"

4. **Pagar matrícula** (POST /PagosCurso/PagarMatriculaCurso)
   - Solo necesario si el estudiante no es becado 100%
   - Soporte para pagos en efectivo y/o tarjeta, en córdobas y/o dólares
   - Calcula vuelto automáticamente
   - Activa la matrícula (cambia a estado "Activa")

5. **Consultar mensualidades pendientes** (GET /PagosCurso/MensualidadesParaPago/{id})
   - Ver lista de mensualidades numeradas (1, 2, 3, etc.)
   - Ver cuáles están pagadas y cuáles pendientes
   - Ver montos con descuento de beca aplicado

6. **Pagar mensualidades** (POST /PagosCurso/PagarMensualidadCurso)
   - Pagar una mensualidad a la vez por su número
   - Becados 100% no necesitan pagar (aparecen como pagadas automáticamente)
   - Soporte multi-moneda igual que matrícula

7. **Marcar curso como aprobado** (PUT /MatriculasCurso/MarcarAprobado/{id})
   - Cuando el estudiante completa el curso
   - Cambia estado a "Completada"
   - Marca como aprobado para evitar re-inscripción

8. **Consultar morosidad** (GET /Reportes/Morosidad)
   - Ver estudiantes morosos de cursos especializados y módulos académicos juntos
   - Mora calculada desde fecha de pago de matrícula

---

## 6. NOTAS IMPORTANTES

### Diferencias clave con módulos académicos:

1. **Sin materias**: Los cursos especializados NO tienen materias/asignaturas individuales
2. **Mensualidades numeradas**: Las mensualidades son simples números (1, 2, 3, etc.) calculados por la duración del curso
3. **Duración automática**: El número de mensualidades = número de meses entre FechaInicio y FechaFin
4. **Independientes del año lectivo**: Los cursos no dependen de años lectivos, tienen sus propias fechas

### Sistema de pagos multi-moneda:

- Todos los montos en la base de datos se almacenan en USD como referencia
- Se permite pagar en cualquier combinación de:
  - Efectivo en córdobas
  - Efectivo en dólares
  - Tarjeta en córdobas
  - Tarjeta en dólares
- El tipo de cambio se registra en cada pago
- El vuelto puede entregarse en cualquier moneda

### Sistema de becas:

- Las becas se configuran a nivel de estudiante (tabla Estudiantes)
- El descuento se aplica automáticamente tanto a matrícula como a mensualidades
- Estudiantes becados al 100%:
  - No pagan matrícula (se activa automáticamente)
  - No pagan mensualidades (aparecen como pagadas automáticamente)
  - No se generan registros de pago para ellos

### Validaciones importantes:

- No se puede desactivar un curso con matrículas activas
- No se puede anular una matrícula con pagos de mensualidad registrados
- No se puede duplicar pago de matrícula
- No se puede duplicar pago de una mensualidad específica
- No se puede re-inscribir en un curso ya aprobado
- El monto pagado debe ser >= al monto requerido

---

## 7. MANEJO DE ERRORES

Todos los endpoints pueden devolver los siguientes códigos de error:

- **400 Bad Request**: Datos inválidos o regla de negocio no cumplida
  - Siempre incluye mensaje descriptivo del error
  - Puede incluir datos adicionales (ej: diferencias de montos)

- **401 Unauthorized**: Token JWT inválido, expirado o no proporcionado
  - Renovar token o iniciar sesión nuevamente

- **404 Not Found**: Recurso no encontrado
  - ID proporcionado no existe en la base de datos

- **500 Internal Server Error**: Error del servidor
  - Incluye mensaje de error y detalles técnicos
  - Contactar soporte técnico

**Formato estándar de error**:
```json
{
  "message": "Descripción del error",
  "error": "Detalles técnicos (solo en 500)",
  "innerError": "Error interno (solo en 500)"
}
```

---

## 8. CONSIDERACIONES DE IMPLEMENTACIÓN

### Para el frontend Vue.js:

1. **Autenticación**:
   - Almacenar token JWT en localStorage o Vuex
   - Incluir en header `Authorization: Bearer {token}` en todas las peticiones
   - Manejar renovación de token o redirigir a login en 401

2. **Gestión de estado**:
   - Considerar Vuex para manejar:
     - Información del usuario autenticado
     - Lista de cursos y matrículas
     - Carrito de pagos

3. **Validaciones**:
   - Validar campos requeridos antes de enviar
   - Validar formato de fechas (YYYY-MM-DD)
   - Validar que FechaInicio < FechaFin
   - Validar montos numéricos positivos
   - Validar tipo de cambio > 0

4. **Cálculos de pago**:
   - Calcular total en USD: `(cordobas / tipoCambio) + dolares`
   - Calcular vuelto: `totalPagado - montoRequerido`
   - Permitir al usuario decidir en qué moneda entregar el vuelto

5. **UX recomendadas**:
   - Mostrar mensualidades en formato de checklist con estado de pago
   - Resaltar mensualidades vencidas o próximas a vencer
   - Mostrar progreso del curso (cuántas mensualidades pagadas de total)
   - Confirmación antes de anular matrículas
   - Feedback visual inmediato después de pagos exitosos

6. **Manejo de fechas**:
   - El backend devuelve fechas en formato ISO 8601
   - Convertir a formato local para mostrar al usuario
   - Usar bibliotecas como day.js o date-fns para manipulación

7. **Manejo de decimales**:
   - Todos los montos son decimales con 2 decimales
   - Usar .toFixed(2) al mostrar montos
   - Evitar problemas de redondeo en cálculos

---

¡Listo! Esta documentación cubre todo el módulo de Cursos Especializados sin referencias a materias/asignaturas.
