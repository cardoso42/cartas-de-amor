# Love Letter - Configuração Docker

Este projeto pode ser executado em dois modos: **desenvolvimento local** e **produção com HTTPS**.

## 🚀 Desenvolvimento Local

Para testes locais, use o arquivo `docker-compose.yml` padrão:

```bash
# Construir e iniciar os containers
docker-compose up --build

# Ou em background
docker-compose up --build -d
```

### Acesso aos serviços:
- **Frontend**: http://localhost:3000
- **API**: http://localhost:8080
- **Banco de dados**: localhost:5433

### Verificar status:
```bash
docker-compose ps
```

### Logs:
```bash
# Todos os serviços
docker-compose logs

# Serviço específico
docker-compose logs frontend
docker-compose logs api
docker-compose logs postgres
```

## 🌐 Produção com HTTPS

Para produção com certificado SSL, use o arquivo `docker-compose.prod.yml`:

### Pré-requisitos:
1. Domínio configurado (ex: `cardoso42.site`)
2. DNS apontando para o servidor
3. Portas 80 e 443 abertas no firewall

### Configuração:

1. **Editar o domínio** no arquivo `setup-ssl.sh`:
```bash
DOMAIN="seu-dominio.com"
EMAIL="seu-email@exemplo.com"
```

2. **Executar o script de configuração**:
```bash
chmod +x setup-ssl.sh
./setup-ssl.sh
```

3. **Ou configurar manualmente**:
```bash
# Iniciar serviços de produção
docker-compose -f docker-compose.prod.yml up -d

# Obter certificado SSL
docker-compose -f docker-compose.prod.yml run --rm certbot certonly \
    --webroot \
    --webroot-path=/var/www/certbot \
    --email seu-email@exemplo.com \
    --agree-tos \
    --no-eff-email \
    -d seu-dominio.com \
    -d www.seu-dominio.com
```

### Acesso em produção:
- **HTTPS**: https://seu-dominio.com
- **HTTP**: http://seu-dominio.com (redireciona para HTTPS)

## 🔧 Comandos Úteis

### Desenvolvimento:
```bash
# Parar todos os containers
docker-compose down

# Parar e remover volumes
docker-compose down -v

# Reconstruir containers
docker-compose up --build

# Ver logs em tempo real
docker-compose logs -f
```

### Produção:
```bash
# Parar serviços de produção
docker-compose -f docker-compose.prod.yml down

# Renovar certificado SSL
docker-compose -f docker-compose.prod.yml run --rm certbot renew

# Recarregar nginx
docker-compose -f docker-compose.prod.yml exec nginx nginx -s reload
```

## 🐛 Solução de Problemas

### Erro "Connection reset":
- Verifique se os containers estão rodando: `docker-compose ps`
- Verifique os logs: `docker-compose logs`
- Certifique-se de que as portas não estão em uso

### Certificado SSL não funciona:
- Verifique se o domínio está apontando para o servidor
- Confirme se as portas 80 e 443 estão abertas
- Verifique os logs do certbot: `docker-compose -f docker-compose.prod.yml logs certbot`

### Frontend não carrega:
- Verifique se a API está funcionando: `curl http://localhost:8080/health`
- Verifique os logs do frontend: `docker-compose logs frontend`

## 📁 Estrutura de Arquivos

```
love-letter/
├── docker-compose.yml          # Desenvolvimento local
├── docker-compose.prod.yml     # Produção com HTTPS
├── setup-ssl.sh               # Script de configuração SSL
├── nginx/
│   ├── conf/
│   │   ├── default.conf       # Configuração para desenvolvimento
│   │   └── production.conf    # Configuração para produção
│   └── certbot/
│       └── conf/              # Certificados SSL
└── README-DOCKER.md           # Esta documentação
```

## 🔒 Segurança

### Desenvolvimento:
- Banco de dados exposto na porta 5433
- Senhas simples para desenvolvimento
- Sem HTTPS

### Produção:
- Banco de dados não exposto externamente
- Certificado SSL válido
- Headers de segurança configurados
- Renovação automática de certificados 