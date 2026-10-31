-- MySQL dump 10.13  Distrib 8.0.46, for Win64 (x86_64)
--
-- Host: 127.0.0.1    Database: shoe_store
-- ------------------------------------------------------
-- Server version	8.0.46

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Current Database: `shoe_store`
--

CREATE DATABASE /*!32312 IF NOT EXISTS*/ `shoe_store` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;

USE `shoe_store`;

--
-- Table structure for table `categories`
--

DROP TABLE IF EXISTS `categories`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `categories` (
  `id` smallint unsigned NOT NULL AUTO_INCREMENT,
  `name` varchar(128) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_categories_name` (`name`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `categories`
--

LOCK TABLES `categories` WRITE;
/*!40000 ALTER TABLE `categories` DISABLE KEYS */;
INSERT INTO `categories` VALUES (1,'Женская обувь'),(2,'Мужская обувь');
/*!40000 ALTER TABLE `categories` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `departments`
--

DROP TABLE IF EXISTS `departments`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `departments` (
  `id` smallint unsigned NOT NULL AUTO_INCREMENT,
  `name` varchar(128) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_departments_name` (`name`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `departments`
--

LOCK TABLES `departments` WRITE;
/*!40000 ALTER TABLE `departments` DISABLE KEYS */;
INSERT INTO `departments` VALUES (1,'Администрация'),(4,'Бухгалтерия'),(2,'Отдел продаж'),(3,'Склад');
/*!40000 ALTER TABLE `departments` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `manufacturers`
--

DROP TABLE IF EXISTS `manufacturers`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `manufacturers` (
  `id` smallint unsigned NOT NULL AUTO_INCREMENT,
  `name` varchar(128) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_manufacturers_name` (`name`)
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `manufacturers`
--

LOCK TABLES `manufacturers` WRITE;
/*!40000 ALTER TABLE `manufacturers` DISABLE KEYS */;
INSERT INTO `manufacturers` VALUES (5,'Alessio Nesca'),(6,'CROSBY'),(1,'Kari'),(2,'Marco Tozzi'),(4,'Rieker'),(3,'Рос');
/*!40000 ALTER TABLE `manufacturers` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `order_items`
--

DROP TABLE IF EXISTS `order_items`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `order_items` (
  `order_id` bigint unsigned NOT NULL,
  `product_id` bigint unsigned NOT NULL,
  `quantity` int NOT NULL,
  `unit_price` decimal(10,2) NOT NULL,
  `discount_percent` decimal(5,2) NOT NULL,
  `line_total` decimal(12,2) NOT NULL,
  PRIMARY KEY (`order_id`,`product_id`),
  KEY `idx_order_items_product` (`product_id`),
  CONSTRAINT `fk_order_items_order` FOREIGN KEY (`order_id`) REFERENCES `orders` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_order_items_product` FOREIGN KEY (`product_id`) REFERENCES `products` (`id`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `chk_order_items_discount` CHECK ((`discount_percent` between 0 and 100)),
  CONSTRAINT `chk_order_items_price` CHECK ((`unit_price` >= 0)),
  CONSTRAINT `chk_order_items_qty` CHECK ((`quantity` > 0)),
  CONSTRAINT `chk_order_items_total` CHECK ((`line_total` = round(((`quantity` * `unit_price`) * (1 - (`discount_percent` / 100))),2)))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `order_items`
--

LOCK TABLES `order_items` WRITE;
/*!40000 ALTER TABLE `order_items` DISABLE KEYS */;
INSERT INTO `order_items` VALUES (1,1,2,4990.00,3.00,9680.60),(1,2,2,3244.00,2.00,6358.24),(2,3,1,4499.00,4.00,4319.04),(2,4,1,5900.00,2.00,5782.00),(3,5,10,3800.00,2.00,37240.00),(3,6,10,4100.00,3.00,39770.00),(4,7,5,2700.00,2.00,13230.00),(4,8,4,1890.00,4.00,7257.60),(5,1,2,4990.00,3.00,9680.60),(5,2,2,3244.00,2.00,6358.24),(6,3,1,4499.00,4.00,4319.04),(6,4,1,5900.00,2.00,5782.00),(7,5,10,3800.00,2.00,37240.00),(7,6,10,4100.00,3.00,39770.00),(8,7,5,2700.00,2.00,13230.00),(8,8,4,1890.00,4.00,7257.60),(9,9,5,4300.00,2.00,21070.00),(9,10,1,2800.00,3.00,2716.00),(10,11,5,2156.00,3.00,10456.60),(10,12,5,1800.00,2.00,8820.00);
/*!40000 ALTER TABLE `order_items` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `order_statuses`
--

DROP TABLE IF EXISTS `order_statuses`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `order_statuses` (
  `id` tinyint unsigned NOT NULL,
  `code` varchar(32) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
  `name` varchar(64) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_order_statuses_code` (`code`),
  UNIQUE KEY `uq_order_statuses_name` (`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `order_statuses`
--

LOCK TABLES `order_statuses` WRITE;
/*!40000 ALTER TABLE `order_statuses` DISABLE KEYS */;
INSERT INTO `order_statuses` VALUES (1,'new','Новый'),(2,'completed','Завершен');
/*!40000 ALTER TABLE `order_statuses` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `orders`
--

DROP TABLE IF EXISTS `orders`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `orders` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `user_id` bigint unsigned NOT NULL,
  `pickup_point_id` smallint unsigned NOT NULL,
  `status_id` tinyint unsigned NOT NULL,
  `order_date` date NOT NULL,
  `delivery_date` date DEFAULT NULL,
  `pickup_code` varchar(6) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
  `total_amount` decimal(12,2) NOT NULL DEFAULT '0.00',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_orders_pickup_code` (`pickup_code`),
  KEY `idx_orders_user_date` (`user_id`,`order_date`),
  KEY `idx_orders_status_date` (`status_id`,`order_date`),
  KEY `idx_orders_date` (`order_date`),
  KEY `idx_orders_pickup` (`pickup_point_id`),
  CONSTRAINT `fk_orders_pickup` FOREIGN KEY (`pickup_point_id`) REFERENCES `pickup_points` (`id`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_orders_status` FOREIGN KEY (`status_id`) REFERENCES `order_statuses` (`id`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_orders_user` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `chk_orders_dates` CHECK (((`delivery_date` is null) or (`delivery_date` >= `order_date`))),
  CONSTRAINT `chk_orders_total` CHECK ((`total_amount` >= 0))
) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `orders`
--

LOCK TABLES `orders` WRITE;
/*!40000 ALTER TABLE `orders` DISABLE KEYS */;
INSERT INTO `orders` VALUES (1,4,1,2,'2025-02-27','2025-04-20','901',16038.84,'2026-10-31 11:50:02','2026-10-31 11:50:02'),(2,1,11,2,'2022-09-28','2025-04-21','902',10101.04,'2026-10-31 11:50:02','2026-10-31 11:50:02'),(3,2,2,2,'2025-03-21','2025-04-22','903',77010.00,'2026-10-31 11:50:02','2026-10-31 11:50:02'),(4,3,11,2,'2025-02-20','2025-04-23','904',20487.60,'2026-10-31 11:50:02','2026-10-31 11:50:02'),(5,4,2,2,'2025-03-17','2025-04-24','905',16038.84,'2026-10-31 11:50:02','2026-10-31 11:50:02'),(6,1,15,2,'2025-03-01','2025-04-25','906',10101.04,'2026-10-31 11:50:02','2026-10-31 11:50:02'),(7,2,3,2,'2025-02-28','2025-04-26','907',77010.00,'2026-10-31 11:50:02','2026-10-31 11:50:02'),(8,3,19,1,'2025-03-31','2025-04-27','908',20487.60,'2026-10-31 11:50:02','2026-10-31 11:50:02'),(9,4,5,1,'2025-04-02','2025-04-28','909',23786.00,'2026-10-31 11:50:02','2026-10-31 11:50:02'),(10,4,19,1,'2025-04-03','2025-04-29','910',19276.60,'2026-10-31 11:50:02','2026-10-31 11:50:02');
/*!40000 ALTER TABLE `orders` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `pickup_points`
--

DROP TABLE IF EXISTS `pickup_points`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `pickup_points` (
  `id` smallint unsigned NOT NULL,
  `postal_code` char(6) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
  `city` varchar(100) NOT NULL,
  `address` varchar(255) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_pickup_points_postal_address` (`postal_code`,`address`),
  KEY `idx_pickup_points_city` (`city`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `pickup_points`
--

LOCK TABLES `pickup_points` WRITE;
/*!40000 ALTER TABLE `pickup_points` DISABLE KEYS */;
INSERT INTO `pickup_points` VALUES (1,'420151','г. Лесной','ул. Вишневая, 32'),(2,'125061','г. Лесной','ул. Подгорная, 8'),(3,'630370','г. Лесной','ул. Шоссейная, 24'),(4,'400562','г. Лесной','ул. Зеленая, 32'),(5,'614510','г. Лесной','ул. Маяковского, 47'),(6,'410542','г. Лесной','ул. Светлая, 46'),(7,'620839','г. Лесной','ул. Цветочная, 8'),(8,'443890','г. Лесной','ул. Коммунистическая, 1'),(9,'603379','г. Лесной','ул. Спортивная, 46'),(10,'603721','г. Лесной','ул. Гоголя, 41'),(11,'410172','г. Лесной','ул. Северная, 13'),(12,'614611','г. Лесной','ул. Молодежная, 50'),(13,'454311','г. Лесной','ул. Новая, 19'),(14,'660007','г. Лесной','ул. Октябрьская, 19'),(15,'603036','г. Лесной','ул. Садовая, 4'),(16,'394060','г. Лесной','ул. Фрунзе, 43'),(17,'410661','г. Лесной','ул. Школьная, 50'),(18,'625590','г. Лесной','ул. Коммунистическая, 20'),(19,'625683','г. Лесной','ул. 8 Марта'),(20,'450983','г. Лесной','ул. Комсомольская, 26'),(21,'394782','г. Лесной','ул. Чехова, 3'),(22,'603002','г. Лесной','ул. Дзержинского, 28'),(23,'450558','г. Лесной','ул. Набережная, 30'),(24,'344288','г. Лесной','ул. Чехова, 1'),(25,'614164','г. Лесной','ул. Степная, 30'),(26,'394242','г. Лесной','ул. Коммунистическая, 43'),(27,'660540','г. Лесной','ул. Солнечная, 25'),(28,'125837','г. Лесной','ул. Шоссейная, 40'),(29,'125703','г. Лесной','ул. Партизанская, 49'),(30,'625283','г. Лесной','ул. Победы, 46'),(31,'614753','г. Лесной','ул. Полевая, 35'),(32,'426030','г. Лесной','ул. Маяковского, 44'),(33,'450375','г. Лесной','ул. Клубная, 44'),(34,'625560','г. Лесной','ул. Некрасова, 12'),(35,'630201','г. Лесной','ул. Комсомольская, 17'),(36,'190949','г. Лесной','ул. Мичурина, 26');
/*!40000 ALTER TABLE `pickup_points` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `products`
--

DROP TABLE IF EXISTS `products`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `products` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `sku` varchar(32) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
  `name` varchar(255) NOT NULL,
  `unit_id` tinyint unsigned NOT NULL,
  `price` decimal(10,2) NOT NULL,
  `supplier_id` smallint unsigned NOT NULL,
  `manufacturer_id` smallint unsigned NOT NULL,
  `category_id` smallint unsigned NOT NULL,
  `discount_percent` decimal(5,2) NOT NULL DEFAULT '0.00',
  `stock_qty` int NOT NULL DEFAULT '0',
  `description` text,
  `image_path` varchar(255) DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_products_sku` (`sku`),
  KEY `idx_products_unit` (`unit_id`),
  KEY `idx_products_category` (`category_id`,`is_active`,`price`),
  KEY `idx_products_supplier` (`supplier_id`,`is_active`,`price`),
  KEY `idx_products_manufacturer` (`manufacturer_id`,`is_active`,`price`),
  KEY `idx_products_active_price` (`is_active`,`price`),
  KEY `idx_products_active_stock` (`is_active`,`stock_qty`),
  KEY `idx_products_name` (`name`),
  CONSTRAINT `fk_products_category` FOREIGN KEY (`category_id`) REFERENCES `categories` (`id`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_products_manufacturer` FOREIGN KEY (`manufacturer_id`) REFERENCES `manufacturers` (`id`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_products_supplier` FOREIGN KEY (`supplier_id`) REFERENCES `suppliers` (`id`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_products_unit` FOREIGN KEY (`unit_id`) REFERENCES `units` (`id`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `chk_products_discount` CHECK ((`discount_percent` between 0 and 100)),
  CONSTRAINT `chk_products_price` CHECK ((`price` >= 0)),
  CONSTRAINT `chk_products_stock` CHECK ((`stock_qty` >= 0))
) ENGINE=InnoDB AUTO_INCREMENT=36 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `products`
--

LOCK TABLES `products` WRITE;
/*!40000 ALTER TABLE `products` DISABLE KEYS */;
INSERT INTO `products` VALUES (1,'A112T4','Ботинки',1,4990.00,1,1,1,3.00,6,'Женские Ботинки демисезонные kari','1.jpg',1,'2026-10-31 11:50:02','2026-10-31 11:50:02'),(2,'F635R4','Ботинки',1,3244.00,2,2,1,2.00,13,'Ботинки Marco Tozzi женские демисезонные, размер 39, цвет бежевый','2.jpg',1,'2026-10-31 11:50:02','2026-10-31 11:50:02'),(3,'H782T5','Туфли',1,4499.00,1,1,2,4.00,5,'Туфли kari мужские классика MYZ21AW-450A, размер 43, цвет: черный','3.jpg',1,'2026-10-31 11:50:02','2026-10-31 11:50:02'),(4,'G783F5','Ботинки',1,5900.00,1,3,2,2.00,8,'Мужские ботинки Рос-Обувь кожаные с натуральным мехом','4.jpg',1,'2026-10-31 11:50:02','2026-10-31 11:50:02'),(5,'J384T6','Ботинки',1,3800.00,2,4,2,2.00,16,'B3430/14 Полуботинки мужские Rieker','5.jpg',1,'2026-10-31 11:50:02','2026-10-31 11:50:02'),(6,'D572U8','Кроссовки',1,4100.00,2,3,2,3.00,6,'129615-4 Кроссовки мужские','6.jpg',1,'2026-10-31 11:50:02','2026-10-31 11:50:02'),(7,'F572H7','Туфли',1,2700.00,1,2,1,2.00,14,'Туфли Marco Tozzi женские летние, размер 39, цвет черный','7.jpg',1,'2026-10-31 11:50:02','2026-10-31 11:50:02'),(8,'D329H3','Полуботинки',1,1890.00,2,5,1,4.00,4,'Полуботинки Alessio Nesca женские 3-30797-47, размер 37, цвет: бордовый','8.jpg',1,'2026-10-31 11:50:02','2026-10-31 11:50:02'),(9,'B320R5','Туфли',1,4300.00,1,4,1,2.00,6,'Туфли Rieker женские демисезонные, размер 41, цвет коричневый','9.jpg',1,'2026-10-31 11:50:02','2026-10-31 11:50:02'),(10,'G432E4','Туфли',1,2800.00,1,1,1,3.00,15,'Туфли kari женские TR-YR-413017, размер 37, цвет: черный','10.jpg',1,'2026-10-31 11:50:02','2026-10-31 11:50:02'),(11,'S213E3','Полуботинки',1,2156.00,2,6,2,3.00,6,'407700/01-01 Полуботинки мужские CROSBY',NULL,1,'2026-10-31 11:50:02','2026-10-31 11:50:02'),(12,'E482R4','Полуботинки',1,1800.00,1,1,1,2.00,14,'Полуботинки kari женские MYZ20S-149, размер 41, цвет: черный',NULL,1,'2026-10-31 11:50:02','2026-10-31 11:50:02'),(13,'S634B5','Кеды',1,5500.00,2,6,2,3.00,0,'Кеды Caprice мужские демисезонные, размер 42, цвет черный',NULL,1,'2026-10-31 11:50:02','2026-10-31 11:50:02'),(14,'K345R4','Полуботинки',1,2100.00,2,6,2,2.00,3,'407700/01-02 Полуботинки мужские CROSBY',NULL,1,'2026-10-31 11:50:02','2026-10-31 11:50:02'),(15,'O754F4','Туфли',1,5400.00,2,4,1,4.00,18,'Туфли женские демисезонные Rieker артикул 55073-68/37',NULL,1,'2026-10-31 11:50:02','2026-10-31 11:50:02'),(16,'G531F4','Ботинки',1,6600.00,1,1,1,12.00,9,'Ботинки женские зимние ROMER арт. 893167-01 Черный',NULL,1,'2026-10-31 11:50:02','2026-10-31 11:50:02'),(17,'J542F5','Тапочки',1,500.00,1,1,2,13.00,0,'Тапочки мужские Арт.70701-55-67син р.41',NULL,1,'2026-10-31 11:50:02','2026-10-31 11:50:02'),(18,'B431R5','Ботинки',1,2700.00,2,4,2,2.00,5,'Мужские кожаные ботинки/мужские ботинки',NULL,1,'2026-10-31 11:50:02','2026-10-31 11:50:02'),(19,'P764G4','Туфли',1,6800.00,1,6,1,15.00,15,'Туфли женские, ARGO, размер 38',NULL,1,'2026-10-31 11:50:02','2026-10-31 11:50:02'),(20,'C436G5','Ботинки',1,10200.00,1,5,1,15.00,9,'Ботинки женские, ARGO, размер 40',NULL,1,'2026-10-31 11:50:02','2026-10-31 11:50:02'),(21,'F427R5','Ботинки',1,11800.00,2,4,1,15.00,11,'Ботинки на молнии с декоративной пряжкой FRAU',NULL,1,'2026-10-31 11:50:02','2026-10-31 11:50:02'),(22,'N457T5','Полуботинки',1,4600.00,1,6,1,3.00,13,'Полуботинки Ботинки черные зимние, мех',NULL,1,'2026-10-31 11:50:02','2026-10-31 11:50:02'),(23,'D364R4','Туфли',1,12400.00,1,1,1,16.00,5,'Туфли Luiza Belly женские Kate-lazo черные из натуральной замши',NULL,1,'2026-10-31 11:50:02','2026-10-31 11:50:02'),(24,'S326R5','Тапочки',1,9900.00,2,6,2,17.00,15,'Мужские кожаные тапочки \"Профиль С.Дали\"',NULL,1,'2026-10-31 11:50:02','2026-10-31 11:50:02'),(25,'L754R4','Полуботинки',1,1700.00,1,1,1,2.00,7,'Полуботинки kari женские WB2020SS-26, размер 38, цвет: черный',NULL,1,'2026-10-31 11:50:02','2026-10-31 11:50:02'),(26,'M542T5','Кроссовки',1,2800.00,2,4,2,18.00,3,'Кроссовки мужские TOFA',NULL,1,'2026-10-31 11:50:02','2026-10-31 11:50:02'),(27,'D268G5','Туфли',1,4399.00,2,4,1,3.00,12,'Туфли Rieker женские демисезонные, размер 36, цвет коричневый',NULL,1,'2026-10-31 11:50:02','2026-10-31 11:50:02'),(28,'T324F5','Сапоги',1,4699.00,1,6,1,2.00,5,'Сапоги замша Цвет: синий',NULL,1,'2026-10-31 11:50:02','2026-10-31 11:50:02'),(29,'K358H6','Тапочки',1,599.00,1,4,2,20.00,2,'Тапочки мужские син р.41',NULL,1,'2026-10-31 11:50:02','2026-10-31 11:50:02'),(30,'H535R5','Ботинки',1,2300.00,2,4,1,2.00,7,'Женские Ботинки демисезонные',NULL,1,'2026-10-31 11:50:02','2026-10-31 11:50:02');
/*!40000 ALTER TABLE `products` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `roles`
--

DROP TABLE IF EXISTS `roles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `roles` (
  `id` tinyint unsigned NOT NULL,
  `code` varchar(32) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
  `name` varchar(64) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_roles_code` (`code`),
  UNIQUE KEY `uq_roles_name` (`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `roles`
--

LOCK TABLES `roles` WRITE;
/*!40000 ALTER TABLE `roles` DISABLE KEYS */;
INSERT INTO `roles` VALUES (1,'admin','Администратор'),(2,'manager','Менеджер'),(3,'client','Авторизированный клиент');
/*!40000 ALTER TABLE `roles` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `suppliers`
--

DROP TABLE IF EXISTS `suppliers`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `suppliers` (
  `id` smallint unsigned NOT NULL AUTO_INCREMENT,
  `name` varchar(128) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_suppliers_name` (`name`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `suppliers`
--

LOCK TABLES `suppliers` WRITE;
/*!40000 ALTER TABLE `suppliers` DISABLE KEYS */;
INSERT INTO `suppliers` VALUES (1,'Kari'),(2,'Обувь для вас');
/*!40000 ALTER TABLE `suppliers` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `units`
--

DROP TABLE IF EXISTS `units`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `units` (
  `id` tinyint unsigned NOT NULL AUTO_INCREMENT,
  `name` varchar(32) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_units_name` (`name`)
) ENGINE=InnoDB AUTO_INCREMENT=32 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `units`
--

LOCK TABLES `units` WRITE;
/*!40000 ALTER TABLE `units` DISABLE KEYS */;
INSERT INTO `units` VALUES (1,'шт.');
/*!40000 ALTER TABLE `units` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `users`
--

DROP TABLE IF EXISTS `users`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `users` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `role_id` tinyint unsigned NOT NULL,
  `department_id` smallint unsigned DEFAULT NULL,
  `full_name` varchar(255) NOT NULL,
  `login` varchar(255) NOT NULL,
  `password_hash` varchar(255) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_users_login` (`login`),
  KEY `idx_users_role_dept_name` (`role_id`,`department_id`,`full_name`),
  KEY `idx_users_dept_name` (`department_id`,`full_name`),
  KEY `idx_users_full_name` (`full_name`),
  CONSTRAINT `fk_users_department` FOREIGN KEY (`department_id`) REFERENCES `departments` (`id`) ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT `fk_users_role` FOREIGN KEY (`role_id`) REFERENCES `roles` (`id`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `chk_users_full_name_len` CHECK ((char_length(`full_name`) between 1 and 255)),
  CONSTRAINT `chk_users_login_len` CHECK ((char_length(`login`) between 3 and 255)),
  CONSTRAINT `chk_users_login_trim` CHECK ((`login` = trim(`login`)))
) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `users`
--

LOCK TABLES `users` WRITE;
/*!40000 ALTER TABLE `users` DISABLE KEYS */;
INSERT INTO `users` VALUES (1,1,1,'Никифорова Весения Николаевна','94d5ous@gmail.com','$2a$12$U0VTXizv6Gq15YX4ydz1a.lribmPkcAA7OIdOGLbeXR5JRGf/TkF.','2026-10-31 11:50:00','2026-10-31 11:50:00'),(2,1,1,'Сазонов Руслан Германович','uth4iz@mail.com','$2a$12$Ydyh8JAwSOHN7b6xf4KLdurtZqe.ojwx2RaUFs1eKucvP1JeOfBMO','2026-10-31 11:50:00','2026-10-31 11:50:00'),(3,1,1,'Одинцов Серафим Артёмович','yzls62@outlook.com','$2a$12$eSbXTZK5lYGWsEstAeC2V.cM1/j80RYrnuJycTfNiqPjBsDbcrmwq','2026-10-31 11:50:00','2026-10-31 11:50:00'),(4,2,2,'Степанов Михаил Артёмович','1diph5e@tutanota.com','$2a$12$vuoamliyQLCtv62VP6Z2feOlCFAr2/OkivFzWxlGKsVk7uk7jp1Vm','2026-10-31 11:50:00','2026-10-31 11:50:00'),(5,2,2,'Ворсин Петр Евгеньевич','tjde7c@yahoo.com','$2a$12$sy37p.5XsGtDWzZ.HTfR6uUvI/qO5xq7lv2YJJCY.xUyBJ2EvP3qe','2026-10-31 11:50:01','2026-10-31 11:50:01'),(6,2,2,'Старикова Елена Павловна','wpmrc3do@tutanota.com','$2a$12$wmNEic9VUhplVDfkOhm23.ikyMH2igjUMl8Kb27UMq3vTuxQhyLue','2026-10-31 11:50:01','2026-10-31 11:50:01'),(7,3,NULL,'Михайлюк Анна Вячеславовна','5d4zbu@tutanota.com','$2a$12$94TAznm/QpHGipk17MFkBOxVOMYyqc59a8vkDpEc3zusZqGX0uAZ6','2026-10-31 11:50:01','2026-10-31 11:50:01'),(8,3,NULL,'Ситдикова Елена Анатольевна','ptec8ym@yahoo.com','$2a$12$OU/dqDrnqenYl92SuZt9wOcuxEnNgO/wacu23BHA8I6knOFwJh5Qu','2026-10-31 11:50:01','2026-10-31 11:50:01'),(9,3,NULL,'Ворсин Петр Евгеньевич','1qz4kw@mail.com','$2a$12$G1YXO2Dgli3LuG2JV4nU1O5l1l2uEMnBBcHGeDUeYIpLBylxt1jDu','2026-10-31 11:50:02','2026-10-31 11:50:02'),(10,3,NULL,'Старикова Елена Павловна','4np6se@mail.com','$2a$12$utFtQh/kh1X5KMXul5w/tuC56AgueHKq.V71w7MMDeyqTMem9db9u','2026-10-31 11:50:02','2026-10-31 11:50:02');
/*!40000 ALTER TABLE `users` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping routines for database 'shoe_store'
--
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-10-31 12:52:13
