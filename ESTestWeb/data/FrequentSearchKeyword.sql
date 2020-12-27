/*
Navicat MySQL Data Transfer

Source Server         : 47.92.101.226_阿里云_root
Source Server Version : 80021
Source Host           : 47.92.101.226:3306
Source Database       : estest

Target Server Type    : MYSQL
Target Server Version : 80021
File Encoding         : 65001

Date: 2020-12-27 15:49:04
*/

SET FOREIGN_KEY_CHECKS=0;

-- ----------------------------
-- Table structure for FrequentSearchKeyword
-- ----------------------------
DROP TABLE IF EXISTS `FrequentSearchKeyword`;
CREATE TABLE `FrequentSearchKeyword` (
  `ESIndexName` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `KeyWord` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Count` int DEFAULT NULL,
  `LastSearchTime` datetime DEFAULT NULL,
  PRIMARY KEY (`ESIndexName`,`KeyWord`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ----------------------------
-- Records of FrequentSearchKeyword
-- ----------------------------
INSERT INTO `FrequentSearchKeyword` VALUES ('qgsourcefile', ' 抗跌', '1', '2020-12-25 13:46:09');
INSERT INTO `FrequentSearchKeyword` VALUES ('qgsourcefile', 'callbackclass  抗跌', '8', '2020-12-25 15:05:20');
INSERT INTO `FrequentSearchKeyword` VALUES ('qgsourcefile', 'callbackclass 抗跌', '6', '2020-12-25 13:50:58');
INSERT INTO `FrequentSearchKeyword` VALUES ('qgsourcefile', 'grpc', '2', '2020-12-25 13:58:13');
INSERT INTO `FrequentSearchKeyword` VALUES ('qgsourcefile', 'price', '8', '2020-12-25 13:56:54');
INSERT INTO `FrequentSearchKeyword` VALUES ('qgsourcefile', '抗跌', '4', '2020-12-25 13:59:51');
INSERT INTO `FrequentSearchKeyword` VALUES ('qgsourcefile', '策略', '4', '2020-12-25 13:48:16');
INSERT INTO `FrequentSearchKeyword` VALUES ('qgsourcefile', '订阅hds行情', '11', '2020-12-25 14:00:07');
INSERT INTO `FrequentSearchKeyword` VALUES ('qgsourcefile', '订阅行情', '10', '2020-12-25 13:58:18');
