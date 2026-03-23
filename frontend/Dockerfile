# Etap 1: Budowanie (Build) - używamy Node.js, aby skompilować Reacta
FROM node:20-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
RUN npm run build
 
# Etap 2: Serwowanie (Production) - używamy lekkiego serweraNginx
FROM nginx:alpine
# Kopiujemy tylko efekt budowania (folder /dist) do folderu Nginx
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]