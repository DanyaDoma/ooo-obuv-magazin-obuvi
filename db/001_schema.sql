-- =====================================================================
-- ООО «Обувь» — схема БД (MySQL 8.0+)
-- Задания 1–4 демоэкзамена.
-- Запуск: mysql -u <user> -p < db/001_schema.sql
-- =====================================================================

CREATE DATABASE IF NOT EXISTS shoe_store
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_0900_ai_ci;

USE shoe_store;

SET NAMES utf8mb4;

-- ---------------------------------------------------------------------
-- 1. Справочники ролей и отделов (задание 4)
-- ---------------------------------------------------------------------
CREATE TABLE roles (
  id    TINYINT UNSIGNED NOT NULL,
  code  VARCHAR(32) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
  name  VARCHAR(64) NOT NULL,
  PRIMARY KEY (id),
  UNIQUE KEY uq_roles_code (code),
  UNIQUE KEY uq_roles_name (name)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE departments (
  id    SMALLINT UNSIGNED NOT NULL AUTO_INCREMENT,
  name  VARCHAR(128) NOT NULL,
  PRIMARY KEY (id),
  UNIQUE KEY uq_departments_name (name)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ---------------------------------------------------------------------
-- 2. Пользователи (задания 2–4)
--    Пароль хранится только как bcrypt-хеш.
--    department_id = NULL допустим для клиентов (не сотрудников).
-- ---------------------------------------------------------------------
CREATE TABLE users (
  id            BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  role_id       TINYINT UNSIGNED NOT NULL,
  department_id SMALLINT UNSIGNED NULL,
  full_name     VARCHAR(255) NOT NULL,
  login         VARCHAR(255) NOT NULL,
  password_hash VARCHAR(255) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
  created_at    DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at    DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  UNIQUE KEY uq_users_login (login),
  KEY idx_users_role_dept_name (role_id, department_id, full_name),
  KEY idx_users_dept_name (department_id, full_name),
  KEY idx_users_full_name (full_name),
  CONSTRAINT fk_users_role
    FOREIGN KEY (role_id) REFERENCES roles (id)
    ON UPDATE CASCADE ON DELETE RESTRICT,
  CONSTRAINT fk_users_department
    FOREIGN KEY (department_id) REFERENCES departments (id)
    ON UPDATE CASCADE ON DELETE SET NULL,
  CONSTRAINT chk_users_login_len
    CHECK (CHAR_LENGTH(login) BETWEEN 3 AND 255),
  CONSTRAINT chk_users_login_trim
    CHECK (login = TRIM(login)),
  CONSTRAINT chk_users_full_name_len
    CHECK (CHAR_LENGTH(full_name) BETWEEN 1 AND 255)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ---------------------------------------------------------------------
-- 3. Справочники товаров
-- ---------------------------------------------------------------------
CREATE TABLE units (
  id    TINYINT UNSIGNED NOT NULL AUTO_INCREMENT,
  name  VARCHAR(32) NOT NULL,
  PRIMARY KEY (id),
  UNIQUE KEY uq_units_name (name)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE categories (
  id    SMALLINT UNSIGNED NOT NULL AUTO_INCREMENT,
  name  VARCHAR(128) NOT NULL,
  PRIMARY KEY (id),
  UNIQUE KEY uq_categories_name (name)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE suppliers (
  id    SMALLINT UNSIGNED NOT NULL AUTO_INCREMENT,
  name  VARCHAR(128) NOT NULL,
  PRIMARY KEY (id),
  UNIQUE KEY uq_suppliers_name (name)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE manufacturers (
  id    SMALLINT UNSIGNED NOT NULL AUTO_INCREMENT,
  name  VARCHAR(128) NOT NULL,
  PRIMARY KEY (id),
  UNIQUE KEY uq_manufacturers_name (name)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ---------------------------------------------------------------------
-- 4. Товары
--    sku — канонический артикул (ASCII, верхний регистр).
--    Исходные артикулы содержат кириллические омоглифы (А, Т и т.п.),
--    поэтому импорт приводит их к латинице до вставки.
--    Удаление — мягкое (is_active = 0), чтобы не рвать историю заказов.
-- ---------------------------------------------------------------------
CREATE TABLE products (
  id               BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  sku              VARCHAR(32) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
  name             VARCHAR(255) NOT NULL,
  unit_id          TINYINT UNSIGNED NOT NULL,
  price            DECIMAL(10,2) NOT NULL,
  supplier_id      SMALLINT UNSIGNED NOT NULL,
  manufacturer_id  SMALLINT UNSIGNED NOT NULL,
  category_id      SMALLINT UNSIGNED NOT NULL,
  discount_percent DECIMAL(5,2) NOT NULL DEFAULT 0.00,
  stock_qty        INT NOT NULL DEFAULT 0,
  description      TEXT NULL,
  image_path       VARCHAR(255) NULL,
  is_active        BOOLEAN NOT NULL DEFAULT TRUE,
  created_at       DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at       DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  UNIQUE KEY uq_products_sku (sku),
  KEY idx_products_unit (unit_id),
  KEY idx_products_category (category_id, is_active, price),
  KEY idx_products_supplier (supplier_id, is_active, price),
  KEY idx_products_manufacturer (manufacturer_id, is_active, price),
  KEY idx_products_active_price (is_active, price),
  KEY idx_products_active_stock (is_active, stock_qty),
  KEY idx_products_name (name),
  CONSTRAINT fk_products_unit
    FOREIGN KEY (unit_id) REFERENCES units (id)
    ON UPDATE CASCADE ON DELETE RESTRICT,
  CONSTRAINT fk_products_supplier
    FOREIGN KEY (supplier_id) REFERENCES suppliers (id)
    ON UPDATE CASCADE ON DELETE RESTRICT,
  CONSTRAINT fk_products_manufacturer
    FOREIGN KEY (manufacturer_id) REFERENCES manufacturers (id)
    ON UPDATE CASCADE ON DELETE RESTRICT,
  CONSTRAINT fk_products_category
    FOREIGN KEY (category_id) REFERENCES categories (id)
    ON UPDATE CASCADE ON DELETE RESTRICT,
  CONSTRAINT chk_products_price CHECK (price >= 0),
  CONSTRAINT chk_products_discount CHECK (discount_percent BETWEEN 0 AND 100),
  CONSTRAINT chk_products_stock CHECK (stock_qty >= 0)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ---------------------------------------------------------------------
-- 5. Пункты выдачи
--    id сохраняет нумерацию из файла заказов (1..36).
-- ---------------------------------------------------------------------
CREATE TABLE pickup_points (
  id          SMALLINT UNSIGNED NOT NULL,
  postal_code CHAR(6) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
  city        VARCHAR(100) NOT NULL,
  address     VARCHAR(255) NOT NULL,
  PRIMARY KEY (id),
  UNIQUE KEY uq_pickup_points_postal_address (postal_code, address),
  KEY idx_pickup_points_city (city)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ---------------------------------------------------------------------
-- 6. Статусы заказов
-- ---------------------------------------------------------------------
CREATE TABLE order_statuses (
  id    TINYINT UNSIGNED NOT NULL,
  code  VARCHAR(32) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
  name  VARCHAR(64) NOT NULL,
  PRIMARY KEY (id),
  UNIQUE KEY uq_order_statuses_code (code),
  UNIQUE KEY uq_order_statuses_name (name)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ---------------------------------------------------------------------
-- 7. Заказы
--    id заказа = номер заказа (в исходных данных 1..10).
--    total_amount — снимок суммы на момент оформления.
--    pickup_code — код получения; уникален.
-- ---------------------------------------------------------------------
CREATE TABLE orders (
  id              BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  user_id         BIGINT UNSIGNED NOT NULL,
  pickup_point_id SMALLINT UNSIGNED NOT NULL,
  status_id       TINYINT UNSIGNED NOT NULL,
  order_date      DATE NOT NULL,
  delivery_date   DATE NULL,
  pickup_code     VARCHAR(6) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
  total_amount    DECIMAL(12,2) NOT NULL DEFAULT 0.00,
  created_at      DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at      DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  UNIQUE KEY uq_orders_pickup_code (pickup_code),
  KEY idx_orders_user_date (user_id, order_date),
  KEY idx_orders_status_date (status_id, order_date),
  KEY idx_orders_date (order_date),
  KEY idx_orders_pickup (pickup_point_id),
  CONSTRAINT fk_orders_user
    FOREIGN KEY (user_id) REFERENCES users (id)
    ON UPDATE CASCADE ON DELETE RESTRICT,
  CONSTRAINT fk_orders_pickup
    FOREIGN KEY (pickup_point_id) REFERENCES pickup_points (id)
    ON UPDATE CASCADE ON DELETE RESTRICT,
  CONSTRAINT fk_orders_status
    FOREIGN KEY (status_id) REFERENCES order_statuses (id)
    ON UPDATE CASCADE ON DELETE RESTRICT,
  CONSTRAINT chk_orders_dates
    CHECK (delivery_date IS NULL OR delivery_date >= order_date),
  CONSTRAINT chk_orders_total CHECK (total_amount >= 0)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ---------------------------------------------------------------------
-- 8. Позиции заказа
--    Цена и скидка — снимок на момент оформления.
-- ---------------------------------------------------------------------
CREATE TABLE order_items (
  order_id         BIGINT UNSIGNED NOT NULL,
  product_id       BIGINT UNSIGNED NOT NULL,
  quantity         INT NOT NULL,
  unit_price       DECIMAL(10,2) NOT NULL,
  discount_percent DECIMAL(5,2) NOT NULL,
  line_total       DECIMAL(12,2) NOT NULL,
  PRIMARY KEY (order_id, product_id),
  KEY idx_order_items_product (product_id),
  CONSTRAINT fk_order_items_order
    FOREIGN KEY (order_id) REFERENCES orders (id)
    ON UPDATE CASCADE ON DELETE CASCADE,
  CONSTRAINT fk_order_items_product
    FOREIGN KEY (product_id) REFERENCES products (id)
    ON UPDATE CASCADE ON DELETE RESTRICT,
  CONSTRAINT chk_order_items_qty CHECK (quantity > 0),
  CONSTRAINT chk_order_items_price CHECK (unit_price >= 0),
  CONSTRAINT chk_order_items_discount CHECK (discount_percent BETWEEN 0 AND 100),
  CONSTRAINT chk_order_items_total
    CHECK (line_total = ROUND(quantity * unit_price * (1 - discount_percent / 100), 2))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
