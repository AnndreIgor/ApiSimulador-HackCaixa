-- Cria banco e usuário só se ainda não existirem
CREATE DATABASE IF NOT EXISTS appdb
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_0900_ai_ci;

CREATE USER IF NOT EXISTS 'appuser'@'%' IDENTIFIED WITH mysql_native_password BY 'App@123';
GRANT ALL PRIVILEGES ON appdb.* TO 'appuser'@'%';

FLUSH PRIVILEGES;
