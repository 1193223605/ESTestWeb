/*
Navicat MySQL Data Transfer

Source Server         : 127.0.0.1
Source Server Version : 80017
Source Host           : localhost:3306
Source Database       : estest

Target Server Type    : MYSQL
Target Server Version : 80017
File Encoding         : 65001

Date: 2020-12-19 19:46:23
*/

SET FOREIGN_KEY_CHECKS=0;

-- ----------------------------
-- Table structure for userinfo
-- ----------------------------
DROP TABLE IF EXISTS `userinfo`;
CREATE TABLE `userinfo` (
  `UserID` varchar(10) NOT NULL,
  `UserName` varchar(50) DEFAULT NULL,
  `Password` varchar(50) DEFAULT NULL,
  `ReserveString` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`UserID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- ----------------------------
-- Records of userinfo
-- ----------------------------
INSERT INTO `userinfo` VALUES ('001', '001用户', '1111', null);
INSERT INTO `userinfo` VALUES ('033', '033用户', '1111', null);
INSERT INTO `userinfo` VALUES ('034', '034用户', '1111', '2020-12-19 18:10:49');
INSERT INTO `userinfo` VALUES ('035', '035user', '1111', '创建时间:2020-12-19 18:14:30');
INSERT INTO `userinfo` VALUES ('036', '036user', '1111', '创建时间:2020-12-19 18:18:22');
INSERT INTO `userinfo` VALUES ('999', '管理员', '1111', null);
