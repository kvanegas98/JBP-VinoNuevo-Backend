# PROMPT PARA FRONTEND - MÓDULO DE AUTENTICACIÓN Y USUARIOS

## CONTEXTO GENERAL
El backend ya está completamente implementado con autenticación JWT. El sistema usa tokens que expiran en 24 horas y todas las validaciones de seguridad están del lado del servidor.

---

## 1. ENDPOINTS DISPONIBLES

### 1.1 AUTENTICACIÓN (AuthController)

#### POST `/api/Auth/Login`
**Propósito**: Autenticar usuario y obtener token JWT

**Request Body**:
```json
{
  "email": "usuario@ejemplo.com",
  "password": "contraseña123"
}
```

**Response Exitosa (200)**:
```json
{
  "usuarioId": 1,
  "nombre": "Juan Pérez",
  "email": "juan@ejemplo.com",
  "rolId": 2,
  "rolNombre": "Administrador",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiracion": "2026-01-11 15:30:00"
}
```

**Errores Posibles**:
- **400**: Datos inválidos (email mal formateado, campos vacíos)
- **401**:
  - "Email o contraseña incorrectos"
  - "Usuario inactivo. Contacte al administrador"
  - "Rol inactivo. Contacte al administrador"
- **500**: Error del servidor

**Qué hacer en el frontend**:
1. Crear formulario con campos email y password
2. Validar formato de email ANTES de enviar
3. Al recibir respuesta exitosa:
   - Guardar el token en localStorage o sessionStorage
   - Guardar información del usuario (usuarioId, nombre, email, rolId, rolNombre)
   - Guardar fecha de expiración
   - Redirigir al dashboard o página principal
4. Al recibir error 401:
   - Mostrar mensaje de error específico al usuario
   - NO intentar reintentos automáticos (evitar bloqueos)
5. Al recibir error 500:
   - Mostrar mensaje genérico de error del servidor

---

#### POST `/api/Auth/ValidarToken`
**Propósito**: Verificar si un token sigue siendo válido (útil al recargar página o después de inactividad)

**Request Body**:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

**Response Exitosa (200)**:
```json
{
  "valido": true,
  "usuario": {
    "usuarioId": 1,
    "nombre": "Juan Pérez",
    "email": "juan@ejemplo.com",
    "rolId": 2,
    "rolNombre": "Administrador"
  }
}
```

**Errores Posibles**:
- **401**: "Token inválido o expirado" o "Token inválido o usuario inactivo"
- **500**: Error del servidor

**Qué hacer en el frontend**:
1. Llamar este endpoint al cargar la aplicación (para verificar sesión existente)
2. Llamar periódicamente cada 5-10 minutos si la app está activa
3. Si responde 200: Continuar con sesión activa
4. Si responde 401: Cerrar sesión, limpiar localStorage, redirigir a login
5. NO mostrar mensajes de error al usuario si el token expiró naturalmente

---

### 1.2 GESTIÓN DE USUARIOS (UsuariosController)

#### GET `/api/Usuarios/Listar`
**Propósito**: Listar usuarios con paginación y filtros

**Query Parameters**:
- `pagina` (int, default: 1): Número de página
- `porPagina` (int, default: 20): Registros por página
- `buscar` (string, opcional): Buscar por nombre o email
- `rolId` (int, opcional): Filtrar por rol específico
- `activo` (bool, opcional): Filtrar por estado (true=activos, false=inactivos)

**Ejemplo URL**:
```
GET /api/Usuarios/Listar?pagina=1&porPagina=20&buscar=juan&rolId=2&activo=true
```

**Response Exitosa (200)**:
```json
{
  "totalRegistros": 45,
  "pagina": 1,
  "porPagina": 20,
  "totalPaginas": 3,
  "datos": [
    {
      "usuarioId": 1,
      "nombre": "Juan Pérez",
      "email": "juan@ejemplo.com",
      "rolId": 2,
      "rolNombre": "Administrador",
      "activo": true
    }
  ]
}
```

**Qué hacer en el frontend**:
1. Crear tabla con columnas: Nombre, Email, Rol, Estado (badge activo/inactivo)
2. Implementar paginación usando `totalPaginas`
3. Mostrar "Mostrando X de Y registros" usando `totalRegistros`
4. Agregar buscador que filtre por nombre o email (input con debounce de 500ms)
5. Agregar filtro por rol (dropdown cargado desde `/api/Roles/Select`)
6. Agregar filtro por estado (toggle activo/inactivo/todos)
7. Agregar botones de acción por fila:
   - Editar (modal o página)
   - Activar/Desactivar (llamar endpoints correspondientes)
   - Cambiar contraseña (solo admins, usar modal)

---

#### GET `/api/Usuarios/Obtener/{id}`
**Propósito**: Obtener información de un usuario específico

**Response Exitosa (200)**:
```json
{
  "usuarioId": 1,
  "nombre": "Juan Pérez",
  "email": "juan@ejemplo.com",
  "rolId": 2,
  "rolNombre": "Administrador",
  "activo": true
}
```

**Errores Posibles**:
- **404**: Usuario no encontrado

**Qué hacer en el frontend**:
1. Llamar este endpoint al abrir modal/página de edición
2. Pre-llenar formulario con los datos recibidos
3. NO mostrar el campo de contraseña (la contraseña se cambia por otro endpoint)

---

#### POST `/api/Usuarios/Crear`
**Propósito**: Crear un nuevo usuario

**Request Body**:
```json
{
  "nombre": "Juan Pérez",
  "email": "juan@ejemplo.com",
  "password": "contraseña123",
  "rolId": 2
}
```

**Validaciones del Backend**:
- Nombre: Requerido, máximo 100 caracteres
- Email: Requerido, formato válido, único en el sistema
- Password: Requerido, mínimo 6 caracteres
- RolId: Requerido, debe existir y estar activo

**Response Exitosa (200)**:
```json
{
  "usuarioId": 15,
  "message": "Usuario creado exitosamente"
}
```

**Errores Posibles**:
- **400**:
  - ModelState inválido (muestra errores de validación)
  - "Ya existe un usuario con ese email"
  - "El rol seleccionado no existe o está inactivo"
- **500**: Error del servidor

**Qué hacer en el frontend**:
1. Crear formulario con campos: Nombre, Email, Contraseña, Confirmar Contraseña, Rol (dropdown)
2. Validaciones ANTES de enviar:
   - Todos los campos requeridos
   - Email con formato válido
   - Contraseña mínimo 6 caracteres
   - Contraseña y Confirmar Contraseña deben coincidir
3. Cargar roles desde `/api/Roles/Select` para el dropdown
4. Al recibir 200: Mostrar mensaje de éxito, cerrar modal, recargar lista
5. Al recibir 400: Mostrar mensaje de error específico debajo del campo correspondiente

---

#### PUT `/api/Usuarios/Actualizar`
**Propósito**: Actualizar información de un usuario (NO incluye contraseña)

**Request Body**:
```json
{
  "usuarioId": 1,
  "nombre": "Juan Carlos Pérez",
  "email": "juancarlos@ejemplo.com",
  "rolId": 2
}
```

**Validaciones del Backend**:
- Mismo que Crear, EXCEPTO password (no se incluye)
- Email único (excepto el mismo usuario)

**Response Exitosa (200)**:
```json
{
  "message": "Usuario actualizado exitosamente"
}
```

**Errores Posibles**:
- **400**: Similar a Crear
- **404**: Usuario no encontrado

**Qué hacer en el frontend**:
1. Formulario similar a Crear PERO SIN campo de contraseña
2. Pre-llenar con datos del endpoint `/api/Usuarios/Obtener/{id}`
3. Mostrar botón separado "Cambiar Contraseña" que abre otro modal

---

#### PUT `/api/Usuarios/CambiarPassword`
**Propósito**: Usuario cambia su propia contraseña (requiere contraseña actual)

**Request Body**:
```json
{
  "usuarioId": 1,
  "passwordActual": "contraseñaVieja123",
  "passwordNueva": "contraseñaNueva456"
}
```

**Validaciones del Backend**:
- PasswordActual: Requerido, debe coincidir con la contraseña actual
- PasswordNueva: Requerido, mínimo 6 caracteres

**Response Exitosa (200)**:
```json
{
  "message": "Contraseña cambiada exitosamente"
}
```

**Errores Posibles**:
- **400**: "La contraseña actual es incorrecta"
- **404**: Usuario no encontrado

**Qué hacer en el frontend**:
1. Crear modal con 3 campos: Contraseña Actual, Nueva Contraseña, Confirmar Nueva Contraseña
2. Validar que Nueva Contraseña y Confirmar coincidan
3. Validar mínimo 6 caracteres
4. Al recibir 200: Cerrar modal, mostrar mensaje de éxito
5. Si el error es "contraseña actual incorrecta": Mostrar error en ese campo específico
6. Este endpoint lo usa el USUARIO para cambiar su propia contraseña

---

#### PUT `/api/Usuarios/ResetPassword`
**Propósito**: Administrador resetea la contraseña de cualquier usuario (NO requiere contraseña actual)

**Request Body**:
```json
{
  "usuarioId": 5,
  "nuevaPassword": "temporal123"
}
```

**Validaciones del Backend**:
- NuevaPassword: Requerido, mínimo 6 caracteres

**Response Exitosa (200)**:
```json
{
  "message": "Contraseña reseteada exitosamente"
}
```

**Qué hacer en el frontend**:
1. Botón "Resetear Contraseña" SOLO visible para administradores
2. Modal simple con 1 campo: Nueva Contraseña Temporal
3. Mensaje de advertencia: "Esta acción cambiará la contraseña del usuario sin su conocimiento"
4. Al recibir 200: Mostrar mensaje con la contraseña temporal para que el admin se la comunique al usuario
5. Este endpoint lo usa el ADMINISTRADOR para resetear contraseñas de otros usuarios

**DIFERENCIA CLAVE**:
- `CambiarPassword`: Usuario cambia SU PROPIA contraseña (requiere contraseña actual)
- `ResetPassword`: Admin resetea contraseña de CUALQUIER USUARIO (no requiere contraseña actual)

---

#### PUT `/api/Usuarios/Activar/{id}`
**Propósito**: Activar un usuario inactivo

**Response Exitosa (200)**:
```json
{
  "message": "Usuario activado exitosamente"
}
```

**Qué hacer en el frontend**:
1. Botón "Activar" solo visible si usuario está inactivo
2. Confirmación: "¿Está seguro que desea activar este usuario?"
3. Al recibir 200: Recargar lista, mostrar mensaje de éxito

---

#### PUT `/api/Usuarios/Desactivar/{id}`
**Propósito**: Desactivar un usuario activo

**Response Exitosa (200)**:
```json
{
  "message": "Usuario desactivado exitosamente"
}
```

**Qué hacer en el frontend**:
1. Botón "Desactivar" solo visible si usuario está activo
2. Confirmación: "¿Está seguro que desea desactivar este usuario? No podrá iniciar sesión"
3. Al recibir 200: Recargar lista, mostrar mensaje de éxito
4. NO permitir desactivar al usuario logueado actualmente

---

### 1.3 GESTIÓN DE ROLES (RolesController)

#### GET `/api/Roles/Listar`
**Propósito**: Listar todos los roles con conteo de usuarios

**Response Exitosa (200)**:
```json
[
  {
    "rolId": 1,
    "nombre": "Administrador",
    "descripcion": "Acceso completo al sistema",
    "activo": true,
    "totalUsuarios": 3
  },
  {
    "rolId": 2,
    "nombre": "Usuario",
    "descripcion": "Acceso limitado",
    "activo": true,
    "totalUsuarios": 12
  }
]
```

**Qué hacer en el frontend**:
1. Crear tabla con columnas: Nombre, Descripción, Estado, Total Usuarios
2. Mostrar badge de estado (activo/inactivo)
3. Agregar botones de acción:
   - Editar
   - Activar/Desactivar (con validación: no se puede desactivar si tiene usuarios activos)
4. NO implementar paginación (generalmente hay pocos roles)

---

#### GET `/api/Roles/Select`
**Propósito**: Obtener lista simplificada de roles activos para dropdowns

**Response Exitosa (200)**:
```json
[
  {
    "id": 1,
    "nombre": "Administrador"
  },
  {
    "id": 2,
    "nombre": "Usuario"
  }
]
```

**Qué hacer en el frontend**:
1. Usar ESTE endpoint para cargar dropdowns de roles (no usar `/Listar`)
2. Cargar al montar componente de crear/editar usuario
3. Cachear en estado local para evitar múltiples llamadas

---

#### GET `/api/Roles/Obtener/{id}`
**Propósito**: Obtener información de un rol específico

**Response Exitosa (200)**:
```json
{
  "rolId": 1,
  "nombre": "Administrador",
  "descripcion": "Acceso completo al sistema",
  "activo": true
}
```

**Qué hacer en el frontend**:
1. Llamar al abrir modal/página de edición
2. Pre-llenar formulario con datos recibidos

---

#### POST `/api/Roles/Crear`
**Propósito**: Crear un nuevo rol

**Request Body**:
```json
{
  "nombre": "Supervisor",
  "descripcion": "Rol con permisos de supervisión"
}
```

**Validaciones del Backend**:
- Nombre: Requerido, máximo 50 caracteres, único
- Descripción: Opcional, máximo 250 caracteres

**Response Exitosa (200)**:
```json
{
  "rolId": 3,
  "nombre": "Supervisor",
  "descripcion": "Rol con permisos de supervisión",
  "activo": true
}
```

**Errores Posibles**:
- **400**:
  - ModelState inválido
  - "Ya existe un rol con ese nombre"

**Qué hacer en el frontend**:
1. Modal con 2 campos: Nombre (requerido), Descripción (opcional)
2. Validar nombre no vacío y máximo 50 caracteres
3. Al recibir 200: Cerrar modal, recargar lista, mostrar éxito

---

#### PUT `/api/Roles/Actualizar`
**Propósito**: Actualizar un rol existente

**Request Body**:
```json
{
  "rolId": 3,
  "nombre": "Supervisor General",
  "descripcion": "Rol actualizado con permisos extendidos"
}
```

**Validaciones del Backend**:
- Similar a Crear
- Nombre único (excepto el mismo rol)

**Response Exitosa (200)**:
```json
{
  "rolId": 3,
  "nombre": "Supervisor General",
  "descripcion": "Rol actualizado con permisos extendidos",
  "activo": true
}
```

**Qué hacer en el frontend**:
1. Formulario similar a Crear
2. Pre-llenar con datos de `/api/Roles/Obtener/{id}`

---

#### PUT `/api/Roles/Activar/{id}`
**Propósito**: Activar un rol inactivo

**Response Exitosa (200)**:
```json
"OK"
```

**Qué hacer en el frontend**:
1. Botón "Activar" solo visible si rol está inactivo
2. Confirmación antes de activar
3. Recargar lista después de 200

---

#### PUT `/api/Roles/Desactivar/{id}`
**Propósito**: Desactivar un rol activo

**Response Exitosa (200)**:
```json
"OK"
```

**Errores Posibles**:
- **400**: "No se puede desactivar el rol porque tiene usuarios activos asociados"

**Qué hacer en el frontend**:
1. Botón "Desactivar" solo visible si rol está activo
2. Confirmación: "¿Está seguro? Los usuarios con este rol no podrán iniciar sesión"
3. Si recibe error 400: Mostrar mensaje específico indicando que tiene usuarios activos
4. Sugerir desactivar primero a los usuarios o cambiarlos de rol

---

## 2. FLUJO DE AUTENTICACIÓN COMPLETO

### 2.1 Al Cargar la Aplicación (App.vue o componente raíz)
```
1. Verificar si existe token en localStorage
2. SI NO existe token:
   - Redirigir a página de login
3. SI existe token:
   - Llamar a /api/Auth/ValidarToken
   - SI responde 200:
     - Guardar datos de usuario en estado global (Vuex/Pinia)
     - Permitir acceso a rutas protegidas
   - SI responde 401:
     - Limpiar localStorage
     - Redirigir a login
```

### 2.2 En la Página de Login
```
1. Mostrar formulario con email y password
2. Al enviar:
   - Validar campos localmente
   - Llamar a /api/Auth/Login
   - SI responde 200:
     - Guardar token en localStorage
     - Guardar datos de usuario en estado global
     - Redirigir al dashboard
   - SI responde 401:
     - Mostrar mensaje de error específico
   - SI responde 400:
     - Mostrar errores de validación
```

### 2.3 En Todas las Peticiones HTTP
```
1. Agregar header: Authorization: Bearer {token}
2. SI respuesta es 401:
   - Significa que el token expiró o es inválido
   - Cerrar sesión automáticamente
   - Redirigir a login
   - Mostrar mensaje: "Sesión expirada, por favor inicie sesión nuevamente"
```

### 2.4 Botón de Cerrar Sesión
```
1. Limpiar localStorage (token, datos de usuario)
2. Limpiar estado global
3. Redirigir a login
4. NO es necesario llamar endpoint del backend
```

---

## 3. MANEJO DE TOKENS JWT

### 3.1 Almacenamiento
- **Dónde**: localStorage (más persistente) o sessionStorage (solo mientras está abierta la pestaña)
- **Qué guardar**:
  ```javascript
  {
    token: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    usuario: {
      usuarioId: 1,
      nombre: "Juan Pérez",
      email: "juan@ejemplo.com",
      rolId: 2,
      rolNombre: "Administrador"
    },
    expiracion: "2026-01-11 15:30:00"
  }
  ```

### 3.2 Uso en Peticiones HTTP
- **Axios Interceptor** (recomendado):
  ```javascript
  axios.interceptors.request.use(config => {
    const token = localStorage.getItem('token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  });
  ```

### 3.3 Manejo de Expiración
- El token expira en 24 horas desde su emisión
- El backend valida la expiración automáticamente
- Si el token expira, el backend responde 401
- Frontend debe cerrar sesión automáticamente al recibir 401

### 3.4 Renovación de Token
- **NO implementada en el backend actual**
- Usuario debe hacer login nuevamente después de 24 horas
- Considerar implementar validación periódica con `/api/Auth/ValidarToken` cada 10 minutos

---

## 4. PROTECCIÓN DE RUTAS POR ROL

### 4.1 Verificación en el Frontend
```javascript
// En el router (Vue Router ejemplo)
router.beforeEach((to, from, next) => {
  const token = localStorage.getItem('token');
  const usuario = JSON.parse(localStorage.getItem('usuario'));

  // Ruta requiere autenticación
  if (to.meta.requiresAuth && !token) {
    return next('/login');
  }

  // Ruta requiere rol específico
  if (to.meta.requiresRole && usuario.rolNombre !== to.meta.requiresRole) {
    return next('/no-autorizado');
  }

  next();
});
```

### 4.2 Ocultación de Elementos en UI
```javascript
// Mostrar opciones solo para admins
<button v-if="esAdministrador">Resetear Contraseña</button>

// En el componente
computed: {
  esAdministrador() {
    const usuario = JSON.parse(localStorage.getItem('usuario'));
    return usuario && usuario.rolNombre === 'Administrador';
  }
}
```

### 4.3 IMPORTANTE
- La protección en el frontend es SOLO para UX
- El backend SIEMPRE valida permisos (aunque no está implementado en estos endpoints)
- Aunque ocultes botones, alguien con conocimientos puede hacer peticiones directas
- Considerar implementar middleware de autorización por rol en el backend

---

## 5. MANEJO DE ERRORES Y MENSAJES

### 5.1 Códigos de Estado HTTP
- **200**: Operación exitosa
- **400**: Datos inválidos (mostrar errores específicos)
- **401**: No autenticado o credenciales incorrectas
- **404**: Recurso no encontrado
- **500**: Error del servidor (mostrar mensaje genérico)

### 5.2 Estructura de Mensajes de Error
```javascript
// Error 400 con ModelState
{
  "message": "Datos inválidos",
  "errors": ["El email es requerido", "La contraseña debe tener al menos 6 caracteres"]
}

// Error 400 simple
{
  "message": "Ya existe un usuario con ese email"
}

// Error 401
{
  "message": "Email o contraseña incorrectos"
}
```

### 5.3 Mostrar Errores en Frontend
```javascript
// Ejemplo de función para manejar errores
function manejarError(error) {
  if (error.response) {
    switch (error.response.status) {
      case 400:
        if (error.response.data.errors) {
          // Mostrar lista de errores
          mostrarErrores(error.response.data.errors);
        } else {
          // Mostrar mensaje único
          mostrarAlerta(error.response.data.message);
        }
        break;
      case 401:
        mostrarAlerta(error.response.data.message);
        // Si no es login, cerrar sesión
        if (!esLoginPage) {
          cerrarSesion();
        }
        break;
      case 404:
        mostrarAlerta('Recurso no encontrado');
        break;
      case 500:
        mostrarAlerta('Error del servidor. Intente nuevamente.');
        break;
    }
  } else {
    mostrarAlerta('Error de conexión. Verifique su internet.');
  }
}
```

---

## 6. CONSIDERACIONES DE SEGURIDAD

### 6.1 Lo que YA está implementado en Backend
✅ Contraseñas hasheadas con HMACSHA512 + salt
✅ Tokens JWT con expiración de 24 horas
✅ Validación de email único
✅ Validación de usuario y rol activos al hacer login
✅ Validación de contraseña actual al cambiar contraseña
✅ No se permite desactivar rol con usuarios activos

### 6.2 Responsabilidades del Frontend
✅ NUNCA guardar contraseñas en texto plano
✅ Limpiar completamente localStorage al cerrar sesión
✅ Validar formato de email antes de enviar
✅ Confirmar acciones destructivas (desactivar, resetear contraseña)
✅ No mostrar mensajes de error que expongan información sensible
✅ Usar HTTPS en producción (el backend usa HTTPS)

### 6.3 Buenas Prácticas
- NO confiar en validaciones solo del frontend
- NO exponer información de usuarios innecesariamente
- NO permitir que usuario desactive su propia cuenta
- Implementar logout automático después de inactividad prolongada
- Mostrar fecha de última sesión (requiere implementar en backend)

---

## 7. ESTRUCTURA DE COMPONENTES SUGERIDA

```
src/
├── views/
│   ├── auth/
│   │   ├── Login.vue                 # Página de login
│   │   └── NoAutorizado.vue          # Página 403
│   ├── usuarios/
│   │   ├── ListaUsuarios.vue         # Tabla con filtros y paginación
│   │   ├── ModalCrearUsuario.vue     # Modal para crear
│   │   ├── ModalEditarUsuario.vue    # Modal para editar
│   │   ├── ModalCambiarPassword.vue  # Modal para cambiar password
│   │   └── ModalResetPassword.vue    # Modal para reset (admin)
│   └── roles/
│       ├── ListaRoles.vue            # Tabla de roles
│       ├── ModalCrearRol.vue         # Modal para crear
│       └── ModalEditarRol.vue        # Modal para editar
├── components/
│   ├── auth/
│   │   └── SessionMonitor.vue        # Componente para validar token periódicamente
│   └── layout/
│       └── NavBar.vue                # Mostrar nombre de usuario, botón logout
├── services/
│   ├── authService.js                # Funciones: login, validarToken, logout
│   ├── usuariosService.js            # Funciones CRUD de usuarios
│   └── rolesService.js               # Funciones CRUD de roles
├── store/
│   └── auth.js                       # Vuex/Pinia store para usuario actual
└── router/
    └── index.js                      # Configuración de rutas y guards
```

---

## 8. EJEMPLOS DE INTEGRACIÓN

### 8.1 Service de Autenticación (authService.js)
```javascript
import axios from 'axios';

const API_URL = 'https://localhost:5001/api/Auth';

export default {
  async login(email, password) {
    const response = await axios.post(`${API_URL}/Login`, {
      email,
      password
    });

    // Guardar token y usuario
    localStorage.setItem('token', response.data.token);
    localStorage.setItem('usuario', JSON.stringify({
      usuarioId: response.data.usuarioId,
      nombre: response.data.nombre,
      email: response.data.email,
      rolId: response.data.rolId,
      rolNombre: response.data.rolNombre
    }));
    localStorage.setItem('expiracion', response.data.expiracion);

    return response.data;
  },

  async validarToken() {
    const token = localStorage.getItem('token');
    const response = await axios.post(`${API_URL}/ValidarToken`, { token });
    return response.data;
  },

  logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('usuario');
    localStorage.removeItem('expiracion');
  },

  getUsuarioActual() {
    const usuario = localStorage.getItem('usuario');
    return usuario ? JSON.parse(usuario) : null;
  },

  estaAutenticado() {
    return !!localStorage.getItem('token');
  },

  esAdministrador() {
    const usuario = this.getUsuarioActual();
    return usuario && usuario.rolNombre === 'Administrador';
  }
};
```

### 8.2 Service de Usuarios (usuariosService.js)
```javascript
import axios from 'axios';

const API_URL = 'https://localhost:5001/api/Usuarios';

export default {
  async listar(filtros = {}) {
    const params = new URLSearchParams();
    if (filtros.pagina) params.append('pagina', filtros.pagina);
    if (filtros.porPagina) params.append('porPagina', filtros.porPagina);
    if (filtros.buscar) params.append('buscar', filtros.buscar);
    if (filtros.rolId) params.append('rolId', filtros.rolId);
    if (filtros.activo !== undefined) params.append('activo', filtros.activo);

    const response = await axios.get(`${API_URL}/Listar?${params}`);
    return response.data;
  },

  async obtener(id) {
    const response = await axios.get(`${API_URL}/Obtener/${id}`);
    return response.data;
  },

  async crear(usuario) {
    const response = await axios.post(`${API_URL}/Crear`, usuario);
    return response.data;
  },

  async actualizar(usuario) {
    const response = await axios.put(`${API_URL}/Actualizar`, usuario);
    return response.data;
  },

  async cambiarPassword(usuarioId, passwordActual, passwordNueva) {
    const response = await axios.put(`${API_URL}/CambiarPassword`, {
      usuarioId,
      passwordActual,
      passwordNueva
    });
    return response.data;
  },

  async resetPassword(usuarioId, nuevaPassword) {
    const response = await axios.put(`${API_URL}/ResetPassword`, {
      usuarioId,
      nuevaPassword
    });
    return response.data;
  },

  async activar(id) {
    const response = await axios.put(`${API_URL}/Activar/${id}`);
    return response.data;
  },

  async desactivar(id) {
    const response = await axios.put(`${API_URL}/Desactivar/${id}`);
    return response.data;
  }
};
```

### 8.3 Axios Interceptor (main.js o archivo de configuración)
```javascript
import axios from 'axios';
import router from './router';
import authService from './services/authService';

// Agregar token a todas las peticiones
axios.interceptors.request.use(
  config => {
    const token = localStorage.getItem('token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  error => {
    return Promise.reject(error);
  }
);

// Manejar errores de autenticación
axios.interceptors.response.use(
  response => response,
  error => {
    if (error.response && error.response.status === 401) {
      // Token expirado o inválido
      authService.logout();
      router.push('/login');
    }
    return Promise.reject(error);
  }
);
```

### 8.4 Router Guard (router/index.js)
```javascript
import authService from '@/services/authService';

const router = new VueRouter({
  routes: [
    {
      path: '/login',
      name: 'Login',
      component: Login
    },
    {
      path: '/dashboard',
      name: 'Dashboard',
      component: Dashboard,
      meta: { requiresAuth: true }
    },
    {
      path: '/usuarios',
      name: 'Usuarios',
      component: ListaUsuarios,
      meta: { requiresAuth: true, requiresRole: 'Administrador' }
    }
  ]
});

router.beforeEach((to, from, next) => {
  const requiresAuth = to.matched.some(record => record.meta.requiresAuth);
  const requiresRole = to.meta.requiresRole;
  const estaAutenticado = authService.estaAutenticado();

  if (requiresAuth && !estaAutenticado) {
    next('/login');
  } else if (requiresRole) {
    const usuario = authService.getUsuarioActual();
    if (usuario && usuario.rolNombre === requiresRole) {
      next();
    } else {
      next('/no-autorizado');
    }
  } else {
    next();
  }
});

export default router;
```

---

## 9. CHECKLIST DE IMPLEMENTACIÓN

### Fase 1: Autenticación Básica
- [ ] Crear página de login con formulario
- [ ] Implementar authService con login, logout, validarToken
- [ ] Configurar axios interceptor para agregar token
- [ ] Guardar datos de usuario en localStorage al login
- [ ] Redirigir a dashboard después de login exitoso
- [ ] Implementar botón de logout en navbar
- [ ] Crear router guard para proteger rutas

### Fase 2: Validación de Sesión
- [ ] Validar token al cargar aplicación
- [ ] Cerrar sesión automáticamente si token es inválido
- [ ] Manejar errores 401 en interceptor de axios
- [ ] Implementar validación periódica de token (opcional)

### Fase 3: Gestión de Usuarios
- [ ] Crear página de lista de usuarios con tabla
- [ ] Implementar paginación y filtros (buscar, rol, estado)
- [ ] Crear modal para crear usuario
- [ ] Crear modal para editar usuario
- [ ] Implementar activar/desactivar usuario
- [ ] Crear modal para cambiar contraseña (usuario)
- [ ] Crear modal para reset de contraseña (admin)

### Fase 4: Gestión de Roles
- [ ] Crear página de lista de roles
- [ ] Crear modal para crear rol
- [ ] Crear modal para editar rol
- [ ] Implementar activar/desactivar rol
- [ ] Validar que no se pueda desactivar rol con usuarios activos

### Fase 5: Protección por Roles
- [ ] Configurar meta en rutas que requieren rol específico
- [ ] Ocultar opciones de admin para usuarios normales
- [ ] Implementar página de "No Autorizado"

### Fase 6: Mejoras de UX
- [ ] Agregar indicadores de carga (spinners)
- [ ] Implementar notificaciones toast para éxitos y errores
- [ ] Agregar confirmaciones para acciones destructivas
- [ ] Mostrar nombre de usuario y rol en navbar
- [ ] Implementar logout automático por inactividad (opcional)

---

## 10. PREGUNTAS FRECUENTES

**Q: ¿Dónde guardo el token?**
A: En localStorage para persistir entre sesiones, o sessionStorage si quieres que expire al cerrar el navegador.

**Q: ¿Cómo sé si el usuario es administrador?**
A: Verifica el campo `rolNombre` en el objeto usuario guardado en localStorage. Si es "Administrador", tiene permisos de admin.

**Q: ¿Qué hago si el token expira mientras el usuario está usando la app?**
A: El backend responderá 401. El interceptor de axios debe detectarlo, cerrar sesión y redirigir a login con mensaje "Sesión expirada".

**Q: ¿Puedo renovar el token sin que el usuario haga login?**
A: NO, el backend actual no tiene endpoint de refresh token. El usuario debe hacer login nuevamente después de 24 horas.

**Q: ¿Cómo diferencio entre "cambiar contraseña" y "resetear contraseña"?**
A: "Cambiar" requiere contraseña actual (usuario lo hace él mismo). "Resetear" NO requiere contraseña actual (admin lo hace a otro usuario).

**Q: ¿Puedo usar este sistema en producción?**
A: El backend tiene seguridad básica implementada. Para producción, considera:
- Usar HTTPS obligatorio
- Implementar refresh tokens
- Agregar rate limiting para prevenir ataques de fuerza bruta
- Implementar log de auditoría de acciones
- Considerar autenticación de dos factores

**Q: ¿Qué pasa si dos admins editan el mismo usuario simultáneamente?**
A: El último en guardar sobrescribe. Para evitarlo, implementar control de concurrencia optimista con timestamps (requiere cambios en backend).

**Q: ¿Los roles tienen permisos específicos?**
A: NO en la implementación actual. Los roles son solo etiquetas. Para implementar permisos granulares (módulos, acciones), se requiere extender el backend con tabla de permisos.

---

## RESUMEN FINAL

Este sistema de autenticación está **completamente implementado en el backend** con:
- Autenticación JWT con tokens de 24 horas
- Gestión completa de usuarios (CRUD, activar/desactivar, cambiar/resetear contraseña)
- Gestión de roles con validaciones
- Seguridad con contraseñas hasheadas

Tu trabajo en el **frontend** es:
1. Crear interfaces de usuario para login y gestión
2. Manejar tokens JWT correctamente
3. Implementar protección de rutas
4. Mostrar/ocultar opciones según rol
5. Manejar errores y validaciones del backend

**Endpoints principales**:
- `POST /api/Auth/Login` - Iniciar sesión
- `POST /api/Auth/ValidarToken` - Verificar token
- `GET /api/Usuarios/Listar` - Lista de usuarios
- `POST /api/Usuarios/Crear` - Crear usuario
- `PUT /api/Usuarios/Actualizar` - Actualizar usuario
- `PUT /api/Usuarios/CambiarPassword` - Cambiar contraseña
- `PUT /api/Usuarios/ResetPassword` - Resetear contraseña (admin)
- `GET /api/Roles/Listar` - Lista de roles
- `GET /api/Roles/Select` - Roles para dropdowns

**El backend ya está listo. Solo necesitas construir el frontend siguiendo este documento.**
