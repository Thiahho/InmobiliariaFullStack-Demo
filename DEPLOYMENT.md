# 🚀 Guía de Deployment - Inmobiliaria Full Stack

## 📋 Requisitos Previos

- ✅ Cuenta en [GitHub](https://github.com)
- ✅ Cuenta en [Vercel](https://vercel.com) (Frontend)
- ✅ Cuenta en [Render](https://render.com) (Backend + Database)
- ✅ Cuenta en [Cloudinary](https://cloudinary.com) (Opcional - para imágenes)

---

## 🔐 IMPORTANTE: Seguridad

**ANTES DE SUBIR A GITHUB:**

1. Verifica que `.gitignore` esté actualizado
2. **NO** subas secretos o contraseñas
3. Los archivos con secretos ya están protegidos:
   - ✅ `.env.local` (ignorado)
   - ✅ `appsettings.Development.json` (ignorado)
   - ✅ `wwwroot/uploads/` (ignorado)

---

## 📦 PASO 1: Preparar el Proyecto

### 1.1 Verificar Archivos de Configuración

**Frontend - `.env.example`:**

```bash
cd frontend
cp .env.example .env.local
# Editar .env.local con tus valores de desarrollo
```

**Backend - Generar JWT Secret:**

```bash
# En PowerShell/CMD, genera un secret aleatorio de 64 caracteres
# Opción 1: Usar un generador online: https://generate-secret.vercel.app/64
# Opción 2: Usar PowerShell
-join ((48..57) + (65..90) + (97..122) | Get-Random -Count 64 | % {[char]$_})
```

### 1.2 Probar Localmente

**Frontend:**

```bash
cd frontend
npm install
npm run build
npm start
```

**Backend:**

```bash
cd LandingBack
dotnet restore
dotnet build
dotnet run
```

Verifica que todo funcione en:

- Frontend: http://localhost:3000
- Backend: http://inmobiliaria-full-stack-demo.vercel.app/swagger

---

## 📤 PASO 2: Subir a GitHub

### 2.1 Crear Repositorio

```bash
# En la raíz del proyecto
git init
git add .
git commit -m "Initial commit - Inmobiliaria Full Stack"

# Crear repositorio en GitHub (via web)
# Luego:
git remote add origin https://github.com/TU-USUARIO/TU-REPO.git
git branch -M main
git push -u origin main
```

**IMPORTANTE:** Revisa que no se hayan subido secretos:

```bash
git log --all --full-history -- "*appsettings.Development.json"
git log --all --full-history -- "*.env.local"
```

---

## 🗄️ PASO 3: Base de Datos en Render

### 3.1 Crear PostgreSQL Database

1. Ve a [Render Dashboard](https://dashboard.render.com)
2. Click **"New +"** → **"PostgreSQL"**
3. Configuración:
   - **Name:** `inmobiliaria-db`
   - **Database:** `inmobiliaria_prod`
   - **User:** (automático)
   - **Region:** `Oregon (US West)` o el más cercano
   - **PostgreSQL Version:** `16`
   - **Plan:** `Free` (para demo)
4. Click **"Create Database"**
5. **Guarda estos valores** (los necesitarás):
   - **Internal Database URL** (para el backend)
   - **External Database URL** (para migraciones manuales)

### 3.2 Aplicar Migraciones

**Opción A: Desde tu máquina local (recomendado para primera vez)**

```bash
# Instalar herramientas EF si no las tienes
dotnet tool install --global dotnet-ef

# Actualizar connection string temporalmente
cd LandingBack

# Editar appsettings.json y poner la External Database URL de Render
# Formato: Host=XXX;Database=YYY;Username=ZZZ;Password=AAA;Port=5432

# Aplicar migraciones
dotnet ef database update

# IMPORTANTE: Revertir appsettings.json a localhost
git checkout appsettings.json
```

**Opción B: Dejar que Render lo haga en el primer deploy (ver paso siguiente)**

---

## 🖥️ PASO 4: Backend en Render

### 4.1 Crear Web Service

1. En Render Dashboard → **"New +"** → **"Web Service"**
2. Conecta tu repositorio de GitHub
3. Configuración:

| Campo              | Valor                  |
| ------------------ | ---------------------- |
| **Name**           | `inmobiliaria-backend` |
| **Region**         | Same as database       |
| **Branch**         | `main`                 |
| **Root Directory** | (dejar vacío)          |
| **Runtime**        | `Docker` o `.NET`      |
| **Build Command**  | Ver abajo              |
| **Start Command**  | Ver abajo              |

**Build Command:**

```bash
cd LandingBack && dotnet restore && dotnet publish -c Release -o out
```

**Start Command:**

```bash
cd LandingBack/out && ./LandingBack --urls "http://0.0.0.0:$PORT"
```

### 4.2 Variables de Entorno

En **"Environment"** tab, agrega:

```bash
# Database (copiar Internal Database URL de Render PostgreSQL)
ConnectionStrings__DefaultConnection=postgresql://user:password@host/database

# JWT - GENERAR NUEVO SECRET (64+ caracteres aleatorios)
Jwt__Key=TU_SECRET_SUPER_SEGURO_DE_64_CARACTERES_MINIMO_AQUI
Jwt__Issuer=InmobiliariaApp
Jwt__Audience=InmobiliariaApp

# Email - Gmail App Password
Email__SmtpHost=smtp.gmail.com
Email__SmtpPort=587
Email__FromName=Inmobiliaria - Sistema de Visitas
Email__FromAddress=tu-email@gmail.com
Email__Username=tu-email@gmail.com
Email__Password=tu_app_password_de_gmail

# Configuración .NET
ASPNETCORE_ENVIRONMENT=Production

# CORS - Lo agregarás después de tener la URL de Vercel
# AllowedOrigins__0=https://tu-app.vercel.app
```

### 4.3 Configurar CORS con Vercel URL

**DESPUÉS de deployar el frontend**, actualiza:

```bash
AllowedOrigins__0=https://tu-app.vercel.app
```

Y en Render → **Environment** → Agrega la variable.

**O actualiza el código directamente** (recomendado):

Edita `appsettings.Production.json`:

```json
{
  "AllowedOrigins": ["https://tu-app.vercel.app"]
}
```

Commit y push:

```bash
git add LandingBack/appsettings.Production.json
git commit -m "Add Vercel URL to CORS"
git push
```

### 4.4 Deploy

Click **"Create Web Service"** → Espera 5-10 minutos

**Verifica el deployment:**

- URL: `https://inmobiliaria-backend.onrender.com`
- Health check: `https://inmobiliaria-backend.onrender.com/health`
- Swagger: `https://inmobiliaria-backend.onrender.com/swagger` (debería estar deshabilitado en producción)

**IMPORTANTE:** Guarda tu URL de Render, la necesitarás para el frontend.

---

## 🌐 PASO 5: Frontend en Vercel

### 5.1 Conectar GitHub

1. Ve a [Vercel Dashboard](https://vercel.com/dashboard)
2. Click **"Add New..." → "Project"**
3. **Import Git Repository** → Selecciona tu repo
4. Configuración:

| Campo                | Valor                                 |
| -------------------- | ------------------------------------- |
| **Framework Preset** | `Next.js` (detectado automáticamente) |
| **Root Directory**   | `frontend`                            |
| **Build Command**    | `npm run build` (default)             |
| **Output Directory** | `.next` (default)                     |
| **Install Command**  | `npm install` (default)               |

### 5.2 Variables de Entorno

En **"Environment Variables"**, agrega:

```bash
NEXT_PUBLIC_API_URL=https://inmobiliaria-backend.onrender.com/api
NEXT_PUBLIC_BASE_URL=https://tu-app.vercel.app
```

**NOTA:** Reemplaza `inmobiliaria-backend` con tu nombre de servicio en Render.

### 5.3 Deploy

Click **"Deploy"** → Espera 2-5 minutos

**URL resultante:** `https://tu-app.vercel.app`

---

## 🔄 PASO 6: Actualizar CORS en Backend

Ahora que tienes la URL de Vercel, actualiza el backend:

### Opción A: Via Environment Variables en Render

1. Ve a Render Dashboard → Tu Web Service
2. Environment → Add Environment Variable:
   ```
   AllowedOrigins__0=https://tu-app.vercel.app
   ```
3. Guarda → El servicio se re-deployará automáticamente

### Opción B: Via Código (Recomendado)

1. Edita `LandingBack/appsettings.Production.json`:

   ```json
   {
     "AllowedOrigins": ["https://tu-app-exacta.vercel.app"]
   }
   ```

2. Commit y push:

   ```bash
   git add LandingBack/appsettings.Production.json
   git commit -m "Update CORS with Vercel URL"
   git push
   ```

3. Render re-deployará automáticamente

---

## ✅ PASO 7: Verificación Final

### 7.1 Checklist de Pruebas

- [ ] Frontend carga correctamente en Vercel
- [ ] Backend responde en `/health`
- [ ] Login funciona desde el frontend
- [ ] CORS no bloquea requests (verifica consola del navegador)
- [ ] Puedes crear/editar propiedades
- [ ] Las imágenes se cargan correctamente
- [ ] Emails de notificación funcionan

### 7.2 Verificar CORS

Abre la consola del navegador (F12) en tu app de Vercel:

- **Sin errores CORS:** ✅ Todo bien
- **Error CORS:** ⚠️ Verifica que la URL de Vercel esté exactamente en `AllowedOrigins`

### 7.3 Monitorear Logs

**Render:**

- Dashboard → Tu servicio → **"Logs"** tab
- Busca errores durante startup

**Vercel:**

- Dashboard → Tu proyecto → **"Deployments"** → Click en deployment → **"Logs"**

---

## 🐛 Troubleshooting

### Problema: CORS Error

**Error en consola:**

```
Access to fetch at 'https://backend.onrender.com/api/...' from origin 'https://app.vercel.app' has been blocked by CORS policy
```

**Solución:**

1. Verifica que `AllowedOrigins__0` esté en Render Environment
2. Verifica que la URL sea EXACTA (con https://, sin trailing slash)
3. Re-deploya el backend

### Problema: Database Connection Failed

**Error en logs:**

```
Npgsql.NpgsqlException: connection refused
```

**Solución:**

1. Verifica que `ConnectionStrings__DefaultConnection` use la **Internal Database URL**
2. Verifica que el formato sea: `Host=XXX;Database=YYY;Username=ZZZ;Password=AAA;Port=5432`
3. No uses la External URL (es para conexiones externas)

### Problema: JWT Token Invalid

**Error en frontend:**

```
401 Unauthorized
```

**Solución:**

1. Verifica que `Jwt__Key` tenga al menos 32 caracteres
2. Verifica que `Jwt__Issuer` y `Jwt__Audience` coincidan
3. Limpia localStorage del navegador: `localStorage.clear()`

### Problema: Imágenes no se guardan

**Síntoma:** Uploads funcionan pero se pierden después del re-deploy

**Solución:**

- Render usa filesystem efímero
- Necesitas integrar Cloudinary, AWS S3, o usar Render Disks (de pago)
- Ver sección siguiente ⬇️

---

## 📸 PASO OPCIONAL: Cloudinary para Imágenes

### ¿Por qué?

En Render, el filesystem se borra con cada deploy. Para persistir imágenes necesitas almacenamiento externo.

### Setup Rápido

1. Crear cuenta en [Cloudinary](https://cloudinary.com) (gratis hasta 25GB)
2. Obtener credenciales: **Cloud Name**, **API Key**, **API Secret**
3. Instalar NuGet en el backend:
   ```bash
   cd LandingBack
   dotnet add package CloudinaryDotNet
   ```
4. Modificar `MediaService.cs` para usar Cloudinary
5. Agregar variables de entorno en Render:
   ```
   Cloudinary__CloudName=tu_cloud_name
   Cloudinary__ApiKey=tu_api_key
   Cloudinary__ApiSecret=tu_api_secret
   ```

---

## 🔄 Workflow de Desarrollo

### Desarrollo Local

```bash
# Frontend
cd frontend
npm run dev

# Backend
cd LandingBack
dotnet watch run
```

### Deploy a Producción

```bash
git add .
git commit -m "Nueva feature: descripción"
git push
```

**Vercel y Render deployarán automáticamente** en cada push a `main`.

---

## 📚 Recursos Útiles

- **Documentación Next.js:** https://nextjs.org/docs
- **Documentación .NET:** https://learn.microsoft.com/aspnet/core
- **Render Docs:** https://render.com/docs
- **Vercel Docs:** https://vercel.com/docs
- **PostgreSQL:** https://www.postgresql.org/docs/

---

## 🆘 Soporte

Si tienes problemas:

1. Revisa los logs en Render y Vercel
2. Verifica las variables de entorno
3. Asegúrate de que las URLs no tengan trailing slashes
4. Verifica que CORS incluya la URL exacta de Vercel

---

## ✨ Mejoras Futuras

- [ ] Integrar Cloudinary para imágenes
- [ ] Configurar dominio personalizado
- [ ] Habilitar Hangfire para jobs background
- [ ] Agregar rate limiting
- [ ] Configurar SSL/TLS personalizado
- [ ] Monitoreo con Sentry o similar
- [ ] CI/CD con GitHub Actions
- [ ] Tests automatizados

---

**¡Felicitaciones! Tu app ya está en producción 🎉**
