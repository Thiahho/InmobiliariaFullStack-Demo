# ✅ Cambios Realizados para Deployment

## 📅 Fecha: $(Get-Date -Format "yyyy-MM-dd")

Este documento resume los cambios realizados para preparar el proyecto para deployment en Vercel y Render.

---

## 🔧 Archivos Modificados

### 1. **LandingBack/Program.cs**

**Cambios:**
- ✅ CORS ahora lee orígenes permitidos desde configuración (variable de entorno `AllowedOrigins`)
- ✅ Validación obligatoria de JWT Key (previene errores de configuración)
- ✅ Valores por defecto para Issuer y Audience

**Antes:**
```csharp
.WithOrigins(
    "http://localhost:3000",
    "https://localhost:3000",
    ...
)
```

**Después:**
```csharp
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:3000" };

corsBuilder.WithOrigins(allowedOrigins)
```

**Beneficio:** Ahora puedes agregar dominios de producción sin modificar código, solo variables de entorno.

---

### 2. **LandingBack/appsettings.json**

**Cambios:**
- ✅ Agregada sección `AllowedOrigins` con localhost por defecto

**Nuevo contenido:**
```json
"AllowedOrigins": [
  "http://localhost:3000",
  "https://localhost:3000",
  "http://localhost:3001",
  "https://localhost:3001",
  "http://localhost:3002",
  "https://localhost:3002"
]
```

**Nota:** Los secretos aún están expuestos aquí (solo para desarrollo local). En producción se usarán variables de entorno.

---

### 3. **.gitignore**

**Cambios:**
- ✅ Protección de archivos sensibles (.env, logs, uploads)
- ✅ Exclusión de archivos de build (.dll, .exe, bin, obj)
- ✅ Exclusión de appsettings.Development.json (con secretos de dev)

**Agregados importantes:**
```
# Environment Variables
.env
.env.local
**/.env

# Uploads (archivos subidos por usuarios)
**/wwwroot/uploads/

# appsettings con secretos
**/appsettings.Development.json
```

---

## 📝 Archivos Nuevos Creados

### 1. **LandingBack/appsettings.Production.json** ⭐

**Propósito:** Configuración para producción SIN secretos expuestos

**Características:**
- Todos los valores sensibles están vacíos (se llenarán con variables de entorno)
- Logging ajustado a nivel "Warning" (menos verbose)
- Solo escribe logs a Console (no a archivos, ya que Render es efímero)

**Uso en Render:**
```bash
# En Environment Variables de Render
ConnectionStrings__DefaultConnection=postgresql://...
Jwt__Key=tu_secret_seguro
Email__FromAddress=tu-email@gmail.com
# etc.
```

---

### 2. **frontend/.env.example**

**Propósito:** Template para variables de entorno del frontend

**Contenido:**
```bash
NEXT_PUBLIC_API_URL=http://localhost:5174/api
NEXT_PUBLIC_BASE_URL=http://localhost:3000
```

**Uso:**
1. Local: `cp .env.example .env.local`
2. Vercel: Copiar valores a Environment Variables

---

### 3. **frontend/.env.local**

**Propósito:** Variables de entorno para desarrollo local

**Contenido:**
```bash
NEXT_PUBLIC_API_URL=http://localhost:5174/api
NEXT_PUBLIC_BASE_URL=http://localhost:3000
```

**IMPORTANTE:** Este archivo está en `.gitignore` y NO se subirá a GitHub.

---

### 4. **DEPLOYMENT.md** 📚

**Propósito:** Guía completa paso a paso para deployment

**Incluye:**
- ✅ Preparación del proyecto
- ✅ Configuración de PostgreSQL en Render
- ✅ Deployment del backend en Render
- ✅ Deployment del frontend en Vercel
- ✅ Configuración de CORS
- ✅ Troubleshooting común
- ✅ Setup opcional de Cloudinary

**Secciones principales:**
1. Requisitos previos
2. Preparar el proyecto
3. Subir a GitHub
4. Base de datos en Render
5. Backend en Render
6. Frontend en Vercel
7. Actualizar CORS
8. Verificación final
9. Troubleshooting

---

## 🚨 IMPORTANTE: Antes de Subir a GitHub

### ❌ NO subir estos archivos con secretos:

- `LandingBack/appsettings.Development.json` (✅ ya está en .gitignore)
- `frontend/.env.local` (✅ ya está en .gitignore)
- `LandingBack/wwwroot/uploads/*` (✅ ya está en .gitignore)

### ✅ SÍ puedes subir:

- `LandingBack/appsettings.json` (template con valores de desarrollo)
- `LandingBack/appsettings.Production.json` (vacío, sin secretos)
- `frontend/.env.example` (template sin valores reales)
- Todo el código fuente

### 🔍 Verificar antes del primer push:

```bash
# Verifica que estos archivos NO aparezcan en el commit
git status

# Si ves archivos sensibles, agrégalos a .gitignore
# Luego:
git rm --cached archivo-sensible.json
git add .gitignore
git commit -m "Update gitignore"
```

---

## 📋 Próximos Pasos

### 1. Generar JWT Secret Seguro

```powershell
# En PowerShell (Windows)
-join ((48..57) + (65..90) + (97..122) | Get-Random -Count 64 | % {[char]$_})
```

O usa: https://generate-secret.vercel.app/64

**Guárdalo para usarlo en Render.**

---

### 2. Configurar Gmail App Password

1. Ve a: https://myaccount.google.com/security
2. Habilita "2-Step Verification"
3. Ve a: https://myaccount.google.com/apppasswords
4. Genera un App Password para "Mail"
5. **Guárdalo para usarlo en Render**

---

### 3. Subir a GitHub

```bash
# Inicializar repo
git init
git add .
git commit -m "Initial commit: Inmobiliaria Full Stack"

# Crear repo en GitHub (via web)
# Luego conectar:
git remote add origin https://github.com/TU-USUARIO/TU-REPO.git
git branch -M main
git push -u origin main
```

---

### 4. Seguir DEPLOYMENT.md

Una vez en GitHub, sigue paso a paso el archivo `DEPLOYMENT.md` para deployar a:
- **Render:** Backend + PostgreSQL
- **Vercel:** Frontend

---

## 🎯 Checklist Final

Antes de deployar, verifica:

### Backend
- [ ] `Program.cs` actualizado con CORS dinámico
- [ ] `appsettings.Production.json` creado (sin secretos)
- [ ] Variables de entorno documentadas
- [ ] Migraciones probadas localmente

### Frontend
- [ ] `.env.example` creado
- [ ] `.env.local` creado (no se sube a GitHub)
- [ ] Build funciona: `npm run build`
- [ ] Axios configurado con `NEXT_PUBLIC_API_URL`

### Seguridad
- [ ] `.gitignore` actualizado
- [ ] JWT Secret generado (64+ caracteres)
- [ ] Gmail App Password generado
- [ ] Sin secretos en código fuente

### GitHub
- [ ] Repositorio creado
- [ ] Código subido
- [ ] Sin archivos sensibles en el repo
- [ ] README.md actualizado

---

## ⚠️ Problemas Conocidos

### 1. Uploads de Imágenes en Render

**Problema:** Render usa filesystem efímero. Los uploads se borrarán con cada deploy.

**Solución:** Integrar Cloudinary o AWS S3 (ver DEPLOYMENT.md).

**Workaround temporal:** Por ahora funciona, pero perderás imágenes en cada re-deploy.

---

### 2. Cold Starts en Render (Plan Free)

**Problema:** En el plan gratuito, si no hay requests por 15 minutos, Render apaga el servidor. El primer request después de eso tardará ~30 segundos.

**Solución:** Usar un plan de pago O implementar un "keep-alive" ping cada 10 minutos.

---

## 📞 Contacto

Si tienes dudas durante el deployment:
1. Revisa `DEPLOYMENT.md` sección Troubleshooting
2. Verifica logs en Render y Vercel
3. Verifica que las URLs no tengan trailing slashes

---

## ✅ Resumen de Archivos

| Archivo | Estado | Descripción |
|---------|--------|-------------|
| `LandingBack/Program.cs` | ✏️ Modificado | CORS dinámico y validación JWT |
| `LandingBack/appsettings.json` | ✏️ Modificado | Agregado AllowedOrigins |
| `LandingBack/appsettings.Production.json` | ✨ Nuevo | Config producción sin secretos |
| `.gitignore` | ✏️ Modificado | Protección de archivos sensibles |
| `frontend/.env.example` | ✨ Nuevo | Template de variables de entorno |
| `frontend/.env.local` | ✨ Nuevo | Variables para desarrollo local |
| `DEPLOYMENT.md` | ✨ Nuevo | Guía completa de deployment |
| `CAMBIOS_DEPLOYMENT.md` | ✨ Nuevo | Este archivo |

---

**¡Tu proyecto ya está listo para deployment! 🚀**

Sigue las instrucciones en `DEPLOYMENT.md` para deployar a Vercel y Render.
