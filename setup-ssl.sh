#!/bin/bash

# Script para configurar SSL com Let's Encrypt
# Uso: ./setup-ssl.sh

DOMAIN="cardoso42.site"
EMAIL="your-email@example.com"  # Substitua pelo seu email

echo "🚀 Configurando SSL para $DOMAIN..."

# 1. Parar containers existentes
echo "📦 Parando containers existentes..."
docker compose -f docker compose.prod.yml down

# 2. Iniciar apenas nginx e certbot
echo "🔧 Iniciando nginx e certbot..."
docker compose -f docker compose.prod.yml up -d nginx certbot

# 3. Obter certificado SSL
echo "🔐 Obtendo certificado SSL..."
docker compose -f docker compose.prod.yml run --rm certbot certonly \
    --webroot \
    --webroot-path=/var/www/certbot \
    --email lucas.cardoso.tech@gmail.com \
    --agree-tos \
    --no-eff-email \
    -d cardoso42.site \
    -d www.cardoso42.site

# 4. Verificar se o certificado foi obtido
if [ -f "./nginx/certbot/conf/live/$DOMAIN/fullchain.pem" ]; then
    echo "✅ Certificado SSL obtido com sucesso!"
    
    # 5. Recarregar nginx
    echo "🔄 Recarregando nginx..."
    docker compose -f docker compose.prod.yml exec nginx nginx -s reload
    
    # 6. Iniciar todos os serviços
    echo "🚀 Iniciando todos os serviços..."
    docker compose -f docker compose.prod.yml up -d
    
    echo "🎉 Configuração SSL concluída!"
    echo "🌐 Acesse: https://$DOMAIN"
else
    echo "❌ Erro ao obter certificado SSL"
    echo "Verifique se:"
    echo "  - O domínio $DOMAIN está apontando para este servidor"
    echo "  - As portas 80 e 443 estão abertas"
    echo "  - O email está configurado corretamente"
    exit 1
fi

# 7. Configurar renovação automática
echo "⏰ Configurando renovação automática..."
(crontab -l 2>/dev/null; echo "0 12 * * * /usr/bin/docker compose -f /path/to/your/docker compose.prod.yml run --rm certbot renew --quiet && /usr/bin/docker compose -f /path/to/your/docker compose.prod.yml exec nginx nginx -s reload") | crontab -

echo "✅ Renovação automática configurada!" 