#!/bin/bash

# Update system
sudo apt-get update
sudo apt-get upgrade -y

# Install .NET SDK
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install -y apt-transport-https
sudo apt-get update
sudo apt-get install -y dotnet-sdk-9.0

# Install Nginx
sudo apt-get install -y nginx

# Configure Nginx
sudo tee /etc/nginx/sites-available/aps << EOF
server {
    listen 80;
    server_name _;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade \$http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host \$host;
        proxy_cache_bypass \$http_upgrade;
    }
}
EOF

# Enable the site
sudo ln -s /etc/nginx/sites-available/aps /etc/nginx/sites-enabled/
sudo rm /etc/nginx/sites-enabled/default
sudo nginx -t
sudo systemctl restart nginx

# Install PostgreSQL
sudo apt-get install -y postgresql postgresql-contrib

# Configure PostgreSQL
sudo -u postgres psql -c "CREATE DATABASE aps;"
sudo -u postgres psql -c "CREATE USER aps_user WITH PASSWORD 'your_password';"
sudo -u postgres psql -c "GRANT ALL PRIVILEGES ON DATABASE aps TO aps_user;"

# Create application directory
sudo mkdir -p /var/www/aps
sudo chown -R \$USER:\$USER /var/www/aps

# Create systemd service
sudo tee /etc/systemd/system/aps.service << EOF
[Unit]
Description=APS .NET Web App
After=network.target

[Service]
WorkingDirectory=/var/www/aps
ExecStart=/usr/bin/dotnet /var/www/aps/APS.dll
Restart=always
RestartSec=10
SyslogIdentifier=aps
User=ubuntu
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DATABASE_URL=Host=localhost;Database=aps;Username=aps_user;Password=your_password
Environment=ADMIN_EMAIL=your-email@example.com
Environment=ADMIN_PASSWORD=your-secure-password

[Install]
WantedBy=multi-user.target
EOF

# Enable and start the service
sudo systemctl enable aps
sudo systemctl start aps 