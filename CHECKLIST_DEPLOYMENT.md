# ✅ Checklist de Deployment - Inmobiliaria Full Stack

## 📋 Preparación (ANTES de subir a GitHub)

### Seguridad
- [ ] Generar JWT Secret de 64+ caracteres
- [ ] Generar Gmail App Password
- [ ] Verificar que `.gitignore` incluye `.env` y `appsettings.Development.json`
- [ ] Revisar que NO haya secretos en archivos que se subirán

### Archivos de Configuración
- [ ] Verificar que existe `frontend/.env.example`
- [ ] Verificar que existe `frontend/.env.local` (para dev local)
- [ ] Verificar que existe `LandingBack/appsettings.Production.json`
- [ ] Verificar que `LandingBack/Program.cs` tiene CORS dinámico

### Pruebas Locales
- [ ] Backend corre sin errores: `cd LandingBack && dotnet run`
- [ ] Frontend corre sin errores: `cd frontend && npm run dev`
- [ ] Frontend puede hacer login al backend
- [ ] Puedes crear/editar una propiedad
- [ ] Backend build exitoso: `dotnet build -c Release`
- [ ] Frontend build exitoso: `npm run build`

---

## 🐙 GitHub

### Crear Repositorio
- [ ] Repositorio creado en GitHub (público o privado)
- [ ] Nombre del repo: `_________________________`
- [ ] URL del repo: `https://github.com/____________/____________`

### Subir Código
```bash
git init
git add .
git commit -m "Initial commit: Inmobiliaria Full Stack"
git remote add origin https://github.com/TU-USUARIO/TU-REPO.git
git branch -M main
git push -u origin main
```

- [ ] Código subido exitosamente
- [ ] Verificar en GitHub web que los archivos estén correctos
- [ ] Verificar que `.env.local` NO está en GitHub
- [ ] Verificar que `appsettings.Development.json` NO está en GitHub
- [ ] Verificar que `wwwroot/uploads/` NO está en GitHub

---

## 🗄️ Render - PostgreSQL Database

### Crear Database
- [ ] Cuenta creada en Render.com
- [ ] New → PostgreSQL
- [ ] Name: `inmobiliaria-db`
- [ ] Database: `inmobiliaria_prod`
- [ ] Region: `_______________` (elegir más cercano)
- [ ] PostgreSQL Version: `16`
- [ ] Plan: `Free` (o el que elijas)

### Guardar Credenciales
- [ ] **Internal Database URL:** `__________________________________________`
- [ ] **External Database URL:** `__________________________________________`

### Aplicar Migraciones

**Opción A - Desde local (recomendado):**
```bash
cd LandingBack
# Temporalmente editar appsettings.json con External URL
dotnet ef database update
# Revertir cambios: git checkout appsettings.json
```
- [ ] Migraciones aplicadas exitosamente
- [ ] Verificar que hay tablas en la DB (usar pgAdmin o psql)

---

## 🖥️ Render - Backend (.NET)

### Crear Web Service
- [ ] New → Web Service
- [ ] Conectar repositorio de GitHub
- [ ] Name: `inmobiliaria-backend` (o el que prefieras)
- [ ] Region: Mismo que la database
- [ ] Branch: `main`
- [ ] Root Directory: (vacío)
- [ ] Runtime: `.NET`

### Build & Start Commands
```bash
# Build Command
cd LandingBack && dotnet restore && dotnet publish -c Release -o out
```
- [ ] Build command configurado

```bash
# Start Command
cd LandingBack/out && ./LandingBack --urls "http://0.0.0.0:$PORT"
```
- [ ] Start command configurado

### Variables de Entorno

#### Database
- [ ] `ConnectionStrings__DefaultConnection` = (Internal Database URL de PostgreSQL)

#### JWT (USAR VALORES GENERADOS, NO ESTOS)
- [ ] `Jwt__Key` = `___________________________________________` (64+ chars)
- [ ] `Jwt__Issuer` = `InmobiliariaApp`
- [ ] `Jwt__Audience` = `InmobiliariaApp`

#### Email
- [ ] `Email__SmtpHost` = `smtp.gmail.com`
- [ ] `Email__SmtpPort` = `587`
- [ ] `Email__FromName` = `Inmobiliaria - Sistema de Visitas`
- [ ] `Email__FromAddress` = `_______________________@gmail.com`
- [ ] `Email__Username` = `_______________________@gmail.com`
- [ ] `Email__Password` = `____________________` (Gmail App Password)

#### .NET
- [ ] `ASPNETCORE_ENVIRONMENT` = `Production`

### Deployment
- [ ] Click "Create Web Service"
- [ ] Esperar 5-10 minutos...
- [ ] Deployment exitoso (status: "Live")
- [ ] URL del backend: `https://_____________________________.onrender.com`

### Verificación
- [ ] Abrir: `https://tu-backend.onrender.com/health`
- [ ] Respuesta: `Healthy` (o similar)
- [ ] Logs no muestran errores críticos

---

## 🌐 Vercel - Frontend (Next.js)

### Conectar GitHub
- [ ] Cuenta creada en Vercel.com
- [ ] Add New... → Project
- [ ] Import Git Repository
- [ ] Seleccionar tu repositorio

### Configuración
- [ ] Framework Preset: `Next.js` (detectado automáticamente)
- [ ] Root Directory: `frontend`
- [ ] Build Command: `npm run build` (default)
- [ ] Output Directory: `.next` (default)
- [ ] Install Command: `npm install` (default)

### Variables de Entorno
- [ ] `NEXT_PUBLIC_API_URL` = `https://TU-BACKEND.onrender.com/api`
- [ ] `NEXT_PUBLIC_BASE_URL` = `https://tu-app.vercel.app`

**IMPORTANTE:** Usa la URL EXACTA de Render sin trailing slash `/`

### Deployment
- [ ] Click "Deploy"
- [ ] Esperar 2-5 minutos...
- [ ] Deployment exitoso
- [ ] URL del frontend: `https://_____________________________.vercel.app`

### Verificación
- [ ] Abrir URL de Vercel
- [ ] La página carga correctamente
- [ ] No hay errores en la consola (F12)

---

## 🔄 Actualizar CORS en Backend

Ahora que tienes la URL de Vercel, actualiza el backend:

### Opción A: Via Variables de Entorno en Render

- [ ] Ir a Render Dashboard → Tu Web Service
- [ ] Environment → Add Environment Variable
- [ ] `AllowedOrigins__0` = `https://tu-app.vercel.app` (URL EXACTA de Vercel)
- [ ] Save Changes (se re-deployará automáticamente)

### Opción B: Via Código (Recomendado)

```bash
# Editar LandingBack/appsettings.Production.json
{
  "AllowedOrigins": [
    "https://tu-app-exacta.vercel.app"
  ]
}

# Commit y push
git add LandingBack/appsettings.Production.json
git commit -m "Add Vercel URL to CORS"
git push
```

- [ ] CORS actualizado
- [ ] Backend re-deployado
- [ ] Verificar que no hay errores CORS en consola del navegador

---

## ✅ Verificación Final

### Frontend
- [ ] Abrir `https://tu-app.vercel.app`
- [ ] Página carga sin errores
- [ ] Sin errores CORS en consola (F12 → Console)
- [ ] Formulario de login visible

### Backend
- [ ] Health check funciona: `https://tu-backend.onrender.com/health`
- [ ] Sin errores en logs de Render

### Integración
- [ ] Login funciona desde frontend
- [ ] Puedes crear una propiedad
- [ ] Las imágenes se suben correctamente
- [ ] Puedes agendar una visita
- [ ] Llega email de confirmación (verificar spam)

### Performance
- [ ] Primera carga del backend < 30 segundos (cold start en Render Free)
- [ ] Cargas subsecuentes < 2 segundos
- [ ] Frontend carga en < 3 segundos

---

## 🎉 ¡Deployment Completado!

### URLs Finales

| Servicio | URL |
|----------|-----|
| **Frontend** | `https://_____________________________.vercel.app` |
| **Backend** | `https://_____________________________.onrender.com` |
| **Database** | (interno en Render) |
| **Health Check** | `https://_____________________________.onrender.com/health` |

### Credenciales de Admin

Para crear el primer usuario admin, ejecuta esto desde tu máquina local:

```bash
# Conectar a la DB de producción
# Usar External Database URL

# O crear via endpoint /api/auth/register con rol Admin
# (requiere modificar código temporalmente para permitir registro de admins)
```

---

## 📊 Monitoreo Post-Deployment

### Diario (Primera Semana)
- [ ] Revisar logs en Render (errores críticos)
- [ ] Verificar que el sitio esté "up" (health check)
- [ ] Probar funcionalidad de login

### Semanal
- [ ] Revisar métricas en Vercel (tiempo de carga)
- [ ] Revisar métricas en Render (tiempo de respuesta)
- [ ] Verificar emails se envían correctamente

### Mensual
- [ ] Revisar uso de PostgreSQL (límites del plan free)
- [ ] Revisar logs de errores acumulados
- [ ] Limpiar datos de prueba si es necesario

---

## 🐛 Si Algo Sale Mal

### CORS Error
**Síntoma:** Error en consola del navegador: "blocked by CORS policy"

**Solución:**
1. Verificar `AllowedOrigins__0` en Render
2. Verificar que la URL de Vercel sea EXACTA (sin trailing `/`)
3. Re-deployar backend

---

### 401 Unauthorized
**Síntoma:** No puedes hacer login

**Solución:**
1. Verificar `Jwt__Key` en Render (debe tener 32+ caracteres)
2. Verificar `Jwt__Issuer` y `Jwt__Audience` coincidan
3. Limpiar localStorage: `localStorage.clear()` en consola

---

### Database Connection Error
**Síntoma:** Logs muestran "connection refused"

**Solución:**
1. Verificar `ConnectionStrings__DefaultConnection` use **Internal** URL (no External)
2. Verificar formato: `Host=XXX;Database=YYY;Username=ZZZ;Password=AAA;Port=5432`
3. Verificar que database esté "Available" en Render

---

### Email No Se Envía
**Síntoma:** No llegan emails de confirmación

**Solución:**
1. Verificar credenciales de Gmail en variables de entorno
2. Verificar que sea App Password (no contraseña normal)
3. Revisar logs del backend para errores SMTP
4. Verificar carpeta de spam

---

## 📝 Notas Adicionales

### Cold Starts en Render Free
- Primera request después de 15 min inactivos puede tardar ~30 segundos
- Es normal en el plan gratuito
- Solución: Upgrade a plan de pago

### Imágenes se Pierden
- Render usa filesystem efímero
- Cada deploy borra `wwwroot/uploads/`
- Solución: Integrar Cloudinary o AWS S3 (ver DEPLOYMENT.md)

### Dominio Personalizado
- En Vercel → Settings → Domains
- Agregar tu dominio (ej: `www.miinmobiliaria.com`)
- Configurar DNS según instrucciones de Vercel

---

**¿Completaste todo? ¡Felicitaciones! 🎊**

Tu aplicación Full Stack ya está en producción y lista para usar.
