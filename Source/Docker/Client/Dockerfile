FROM node:20-alpine3.20 AS build

WORKDIR /app

# Copy package files
COPY Client/pot-react/package*.json ./

# Install dependencies
RUN npm ci

# Copy the rest of the application code
COPY Client/pot-react/ .

# Build the application with production environment
ENV NODE_ENV=production
RUN npm run build

# Production stage
FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html

# Copy our custom nginx configuration
COPY Docker/Client/nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
