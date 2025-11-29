# Guía de Despliegue en PaaS

Esta guía explica cómo desplegar la aplicación en diferentes plataformas como servicio (PaaS).

## Configuración Requerida

Antes de desplegar, asegúrate de tener configuradas las siguientes variables de entorno:

- `SUPABASE_URL`: URL de tu proyecto Supabase
- `SUPABASE_KEY`: Clave anónima (anon key) de Supabase
- `PORT`: Puerto donde la aplicación escuchará (opcional, por defecto 8080)
- `USE_HTTPS_REDIRECTION`: Si está en "false", desactiva la redirección HTTPS (útil cuando el PaaS maneja HTTPS)

## Plataformas Soportadas

### 1. Railway

1. Conecta tu repositorio GitHub a Railway
2. Railway detectará automáticamente el Dockerfile
3. Configura las variables de entorno:
   - `SUPABASE_URL`
   - `SUPABASE_KEY`
   - `USE_HTTPS_REDIRECTION=false` (Railway maneja HTTPS)
4. Despliega

**Nota**: Railway automáticamente configurará el puerto, no necesitas configurar `PORT`.

### 2. Render

1. Ve a [Render Dashboard](https://dashboard.render.com/)
2. Crea un nuevo "Web Service"
3. Conecta tu repositorio
4. Configura:
   - **Build Command**: `docker build -t supabasenet .`
   - **Start Command**: `docker run -p $PORT:8080 -e SUPABASE_URL=$SUPABASE_URL -e SUPABASE_KEY=$SUPABASE_KEY supabasenet`
   - O simplemente deja que Render detecte el Dockerfile automáticamente
5. Configura las variables de entorno:
   - `SUPABASE_URL`
   - `SUPABASE_KEY`
   - `USE_HTTPS_REDIRECTION=false`
6. Despliega

### 3. Heroku

1. Instala el [Heroku CLI](https://devcenter.heroku.com/articles/heroku-cli)
2. Login: `heroku login`
3. Crea una app: `heroku create tu-app-nombre`
4. Configura las variables de entorno:
   ```bash
   heroku config:set SUPABASE_URL=tu_url
   heroku config:set SUPABASE_KEY=tu_key
   heroku config:set USE_HTTPS_REDIRECTION=false
   ```
5. Despliega:
   ```bash
   git push heroku main
   ```

**Nota**: Necesitarás un `heroku.yml` si usas Docker. Alternativamente, Heroku puede usar buildpacks de .NET.

### 4. Azure Container Instances (ACI)

1. Construye la imagen:
   ```bash
   docker build -t supabasenet .
   ```
2. Sube la imagen a Azure Container Registry
3. Crea una instancia de contenedor con las variables de entorno configuradas

### 5. Google Cloud Run

1. Construye y sube la imagen:
   ```bash
   gcloud builds submit --tag gcr.io/tu-proyecto/supabasenet
   ```
2. Despliega:
   ```bash
   gcloud run deploy supabasenet \
     --image gcr.io/tu-proyecto/supabasenet \
     --platform managed \
     --set-env-vars SUPABASE_URL=tu_url,SUPABASE_KEY=tu_key \
     --allow-unauthenticated
   ```

### 6. AWS App Runner / ECS Fargate

1. Construye la imagen Docker
2. Sube a Amazon ECR
3. Crea un servicio en App Runner o ECS con las variables de entorno configuradas

### 7. Fly.io

1. Instala Fly CLI: `curl -L https://fly.io/install.sh | sh`
2. Login: `fly auth login`
3. Inicializa: `fly launch`
4. Configura variables de entorno:
   ```bash
   fly secrets set SUPABASE_URL=tu_url
   fly secrets set SUPABASE_KEY=tu_key
   fly secrets set USE_HTTPS_REDIRECTION=false
   ```
5. Despliega: `fly deploy`

### 8. DigitalOcean App Platform

1. Ve a [DigitalOcean Apps](https://cloud.digitalocean.com/apps)
2. Crea una nueva app desde GitHub
3. Selecciona el repositorio y branch
4. DigitalOcean detectará el Dockerfile automáticamente
5. Configura las variables de entorno en la sección de configuración
6. Despliega

## Construcción Local del Dockerfile

Para probar el Dockerfile localmente:

```bash
# Construir la imagen
docker build -t supabasenet .

# Ejecutar el contenedor
docker run -p 8080:8080 \
  -e SUPABASE_URL=tu_url \
  -e SUPABASE_KEY=tu_key \
  supabasenet
```

O usando docker-compose (crea un `docker-compose.yml`):

```yaml
version: '3.8'
services:
  app:
    build: .
    ports:
      - "8080:8080"
    environment:
      - SUPABASE_URL=${SUPABASE_URL}
      - SUPABASE_KEY=${SUPABASE_KEY}
      - USE_HTTPS_REDIRECTION=false
```

Luego ejecuta:
```bash
docker-compose up
```

## Variables de Entorno vs appsettings.json

La aplicación busca las credenciales de Supabase en este orden:

1. Variables de entorno: `SUPABASE_URL` y `SUPABASE_KEY`
2. Configuración en `appsettings.json`: `Supabase:Url` y `Supabase:Key`

**Recomendación**: En producción, usa siempre variables de entorno para mayor seguridad.

## Sesiones en Producción

En producción, se recomienda usar un almacenamiento de sesiones distribuido (como Redis) en lugar de `DistributedMemoryCache`. Esto es especialmente importante si tienes múltiples instancias de la aplicación.

Para mejorar esto en el futuro, puedes usar:
- Redis con `StackExchange.Redis`
- SQL Server con sesiones distribuidas
- Azure Cache for Redis

## Verificación Post-Despliegue

Después de desplegar, verifica:

1. ✅ La aplicación responde en la URL proporcionada
2. ✅ Puedes registrarte e iniciar sesión
3. ✅ Puedes crear, editar y eliminar libros
4. ✅ Las imágenes se suben y muestran correctamente
5. ✅ Los logs no muestran errores de conexión a Supabase

## Solución de Problemas

### Error: "Las credenciales de Supabase no están configuradas"
- Verifica que las variables de entorno `SUPABASE_URL` y `SUPABASE_KEY` estén configuradas
- Verifica que los nombres de las variables sean exactamente `SUPABASE_URL` y `SUPABASE_KEY`

### Error: "Port already in use"
- Asegúrate de que el PaaS esté configurando la variable `PORT` correctamente
- Algunos PaaS usan variables diferentes, verifica la documentación específica

### Las imágenes no se cargan
- Verifica que el bucket de Storage esté configurado como público
- Verifica que las políticas de Storage estén configuradas correctamente
- Revisa los logs para ver errores específicos de Storage

### Problemas de sesión
- En producción con múltiples instancias, considera usar un almacenamiento de sesiones distribuido
- Verifica que las cookies funcionen correctamente en tu dominio

## Recursos Adicionales

- [Documentación de Supabase Storage](https://supabase.com/docs/guides/storage)
- [Documentación de ASP.NET Core en Docker](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/docker/)
- [Railway Documentation](https://docs.railway.app/)
- [Render Documentation](https://render.com/docs)
