-- MySQL dump 10.13  Distrib 8.0.19, for Win64 (x86_64)
--
-- Host: localhost    Database: tts
-- ------------------------------------------------------
-- Server version	8.0.19

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `station`
--

DROP TABLE IF EXISTS `station`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `station` (
  `trainid` varchar(10) NOT NULL,
  `stationid` int unsigned NOT NULL,
  `stationname` varchar(30) DEFAULT NULL,
  `entertime` time DEFAULT NULL,
  `leavetime` time DEFAULT NULL,
  PRIMARY KEY (`trainid`,`stationid`),
  CONSTRAINT `fk_train_trainid` FOREIGN KEY (`trainid`) REFERENCES `train` (`trainid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `station`
--

LOCK TABLES `station` WRITE;
/*!40000 ALTER TABLE `station` DISABLE KEYS */;
INSERT INTO `station` VALUES ('K123',1,'上海南','13:40:00','13:40:00'),('K123',2,'松江','13:59:00','14:03:00'),('K123',3,'嘉善','14:25:00','14:28:00'),('K123',4,'海宁','14:58:00','15:02:00'),('K123',5,'杭州南','16:02:00','16:17:00'),('K586',1,'深圳西','08:50:00','08:50:00'),('K586',2,'东莞','10:02:00','10:05:00'),('K586',3,'广州','11:16:00','11:36:00'),('K586',4,'佛山','12:00:00','12:04:00'),('K586',5,'肇庆','13:21:00','13:27:00'),('Z001',1,'杭州南','08:00:00','08:05:00'),('Z001',2,'广州','15:00:00','15:10:00');
/*!40000 ALTER TABLE `station` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `ticket`
--

DROP TABLE IF EXISTS `ticket`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `ticket` (
  `ticketid` int NOT NULL,
  `startstation` varchar(30) DEFAULT NULL,
  `starttime` time DEFAULT NULL,
  `endstation` varchar(30) DEFAULT NULL,
  `arrivaltime` time DEFAULT NULL,
  `trainid` varchar(10) NOT NULL,
  `price` float DEFAULT NULL,
  PRIMARY KEY (`ticketid`),
  KEY `fk_train_trianid_idx` (`trainid`),
  CONSTRAINT `fk_train_trianid` FOREIGN KEY (`trainid`) REFERENCES `train` (`trainid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `ticket`
--

LOCK TABLES `ticket` WRITE;
/*!40000 ALTER TABLE `ticket` DISABLE KEYS */;
INSERT INTO `ticket` VALUES (1,'深圳西','08:50:00','东莞','10:02:00','K586',10),(2,'深圳西','08:50:00','广州','11:16:00','K586',11),(3,'深圳西','08:50:00','佛山','12:00:00','K586',12),(4,'深圳西','08:50:00','肇庆','13:21:00','K586',13),(5,'东莞','10:05:00','广州','11:16:00','K586',10),(6,'东莞','10:05:00','佛山','12:00:00','K586',10),(7,'东莞','10:05:00','肇庆','13:21:00','K586',10),(8,'广州','11:36:00','佛山','12:00:00','K586',10),(9,'广州','11:36:00','肇庆','13:21:00','K586',10),(10,'佛山','12:04:00','肇庆','13:21:00','K586',10),(11,'上海南','13:40:00','松江','13:59:00','K123',10),(12,'上海南','13:40:00','嘉善','14:25:00','K123',11),(13,'上海南','13:40:00','海宁','14:58:00','K123',12),(14,'上海南','13:40:00','杭州南','16:02:00','K123',13),(15,'松江','14:03:00','嘉善','14:25:00','K123',10),(16,'松江','14:03:00','海宁','14:58:00','K123',10),(17,'松江','14:03:00','杭州南','16:02:00','K123',10),(18,'嘉善','14:28:00','海宁','15:02:00','K123',10),(19,'嘉善','14:28:00','杭州南','16:17:00','K123',10),(20,'海宁','15:02:00','杭州南','16:17:00','K123',10);
/*!40000 ALTER TABLE `ticket` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `torder`
--

DROP TABLE IF EXISTS `torder`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `torder` (
  `orderid` int NOT NULL,
  `userid` int NOT NULL,
  `ticketid` int NOT NULL,
  `orderdate` date DEFAULT NULL,
  PRIMARY KEY (`orderid`),
  KEY `userid_idx` (`userid`),
  KEY `fk_ticket_ticketid_idx` (`ticketid`),
  CONSTRAINT `fk_ticket_ticketid` FOREIGN KEY (`ticketid`) REFERENCES `ticket` (`ticketid`),
  CONSTRAINT `fk_user_userid` FOREIGN KEY (`userid`) REFERENCES `user` (`UserID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `torder`
--

LOCK TABLES `torder` WRITE;
/*!40000 ALTER TABLE `torder` DISABLE KEYS */;
INSERT INTO `torder` VALUES (2008201929,102,18,'2020-08-20'),(2008211925,101,3,'2020-08-21');
/*!40000 ALTER TABLE `torder` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `train`
--

DROP TABLE IF EXISTS `train`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `train` (
  `trainid` varchar(10) NOT NULL,
  `seatcount` int unsigned DEFAULT NULL,
  PRIMARY KEY (`trainid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `train`
--

LOCK TABLES `train` WRITE;
/*!40000 ALTER TABLE `train` DISABLE KEYS */;
INSERT INTO `train` VALUES ('K123',1500),('K586',1000),('Z001',200);
/*!40000 ALTER TABLE `train` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `traincanceled`
--

DROP TABLE IF EXISTS `traincanceled`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `traincanceled` (
  `trainid` varchar(10) NOT NULL,
  `startdate` date NOT NULL,
  `enddata` date DEFAULT NULL,
  PRIMARY KEY (`trainid`,`startdate`),
  CONSTRAINT `fk_traincancel_train` FOREIGN KEY (`trainid`) REFERENCES `train` (`trainid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `traincanceled`
--

LOCK TABLES `traincanceled` WRITE;
/*!40000 ALTER TABLE `traincanceled` DISABLE KEYS */;
INSERT INTO `traincanceled` VALUES ('K123','2020-08-25','2020-08-25');
/*!40000 ALTER TABLE `traincanceled` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `user`
--

DROP TABLE IF EXISTS `user`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `user` (
  `UserID` int NOT NULL,
  `UserName` varchar(20) NOT NULL,
  `UserPho` char(15) DEFAULT NULL,
  `balance` float DEFAULT NULL,
  `usertype` char(1) DEFAULT NULL,
  PRIMARY KEY (`UserID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `user`
--

LOCK TABLES `user` WRITE;
/*!40000 ALTER TABLE `user` DISABLE KEYS */;
INSERT INTO `user` VALUES (101,'Mike','13088008800',100,'Y'),(102,'Nike','15988008800',5,'Y'),(103,'张三','12003000300',0,'G');
/*!40000 ALTER TABLE `user` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2020-08-21 22:49:30
