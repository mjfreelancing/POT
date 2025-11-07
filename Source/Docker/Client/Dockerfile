FROM node:20-alpine3.20 AS build

# Build arguments for Vite environment variables
ARG VITE_API_BASE_URL
ARG VITE_API_TIMEOUT_MS

WORKDIR /app

# Copy package files
COPY Client/pot-react/package*.json ./

# Install dependencies
RUN npm ci

# Copy the rest of the application code
COPY Client/pot-react/ .

# Build the application with production environment
# For local Docker builds: Uses .env.production file (VITE_API_BASE_URL=/api)
# For Azure builds: ARG values override .env.production via --build-arg
ENV NODE_ENV=production
# Only set ENV if ARG is provided (Azure builds), otherwise use .env.production
RUN if [ -n "$VITE_API_BASE_URL" ]; then \
      export VITE_API_BASE_URL=$VITE_API_BASE_URL; \
      export VITE_API_TIMEOUT_MS=$VITE_API_TIMEOUT_MS; \
    fi && \
    npm run build

# Production stage
FROM nginx:alpine

# Build argument to select nginx config (default to local Docker Compose config)
ARG NGINX_CONFIG=nginx.conf

COPY --from=build /app/dist /usr/share/nginx/html

# Copy the appropriate nginx configuration based on build argument
COPY Docker/Client/${NGINX_CONFIG} /etc/nginx/conf.d/default.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
