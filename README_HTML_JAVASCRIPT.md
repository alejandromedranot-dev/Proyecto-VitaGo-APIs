# Sistema de empleados con API .NET 8 y JavaScript puro

Esta versión ya no utiliza Angular. Incluye login didáctico, home principal y CRUD de empleados. El frontend está dentro de `MiWebAPI/MiWebAPI/wwwroot` y se publica desde la aplicación .NET.

## Estructura principal

```text
MiWebAPI
└── MiWebAPI
    ├── Controllers
    ├── Data
    ├── Models
    ├── wwwroot
    │   ├── login.html
    │   ├── index.html (home)
    │   ├── empleados.html (CRUD)
    │   ├── css/styles.css
    │   └── js
    │       ├── auth.js
    │       ├── login.js
    │       ├── config.js
    │       └── app.js
    └── Program.cs
Scripts.sql
```

## Ejecución

1. Ejecuta `Scripts.sql` en SQL Server.
2. Configura `CadenaSQL` en `appsettings.json`.
3. Abre la solución `MiWebAPI/MiWebAPI.sln`.
4. Ejecuta el perfil HTTP de la API.
5. Abre `http://localhost:5299` en el navegador. Si no hay una sesión activa, la aplicación te enviará al login.

## Acceso de demostración

- Correo: `admin@demo.com`
- Contraseña: `Admin123*`

Los datos se configuran en `DemoLogin` dentro de `appsettings.json`. El endpoint de acceso es `POST /api/Auth/login`.

> Este login es deliberadamente sencillo para fines académicos. La sesión se conserva en `sessionStorage`; no utiliza JWT ni protege los endpoints de la API. Para producción se debe implementar autenticación y autorización seguras.

Swagger continúa disponible en `http://localhost:5299/swagger` cuando el ambiente es Development.

## Respuesta de las operaciones

Los procedimientos de creación, actualización y eliminación regresan:

```sql
@numeroError INT OUTPUT,
@respuesta VARCHAR(1000) OUTPUT
```

La API transforma esos valores en una respuesta como:

```json
{
  "numeroError": 0,
  "respuesta": "Empleado agregado correctamente",
  "isSuccess": true
}
```
