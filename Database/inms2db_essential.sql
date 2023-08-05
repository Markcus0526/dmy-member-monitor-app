# Host: 192.168.1.104  (Version: 5.5.16)
# Date: 2012-07-10 14:43:33
# Generator: MySQL-Front 5.2  (Build 2.5)

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE */;
/*!40101 SET SQL_MODE='' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES */;
/*!40103 SET SQL_NOTES='ON' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS */;
/*!40014 SET FOREIGN_KEY_CHECKS=0 */;

#
# Source for table "tbl1appdisable_set"
#

DROP TABLE IF EXISTS `tbl1appdisable_set`;
CREATE TABLE `tbl1appdisable_set` (
  `uid` int(11) NOT NULL AUTO_INCREMENT,
  `userid` int(11) DEFAULT NULL,
  `disable` bit(1) DEFAULT NULL,
  `mon_time` datetime DEFAULT NULL,
  `name` varchar(255) DEFAULT NULL,
  `description` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`uid`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8;

#
# Data for table "tbl1appdisable_set"
#

/*!40000 ALTER TABLE `tbl1appdisable_set` DISABLE KEYS */;
/*!40000 ALTER TABLE `tbl1appdisable_set` ENABLE KEYS */;

#
# Source for table "tbl1prohibit_set"
#

DROP TABLE IF EXISTS `tbl1prohibit_set`;
CREATE TABLE `tbl1prohibit_set` (
  `uid` int(11) NOT NULL AUTO_INCREMENT,
  `userid` int(11) DEFAULT NULL,
  `mon_time` datetime DEFAULT NULL,
  `usb_disable` bit(1) DEFAULT NULL,
  `bandup_width` int(11) DEFAULT NULL,
  `banddown_width` int(11) DEFAULT NULL,
  PRIMARY KEY (`uid`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8;

#
# Data for table "tbl1prohibit_set"
#

/*!40000 ALTER TABLE `tbl1prohibit_set` DISABLE KEYS */;
/*!40000 ALTER TABLE `tbl1prohibit_set` ENABLE KEYS */;

#
# Source for table "tbl2net_set"
#

DROP TABLE IF EXISTS `tbl2net_set`;
CREATE TABLE `tbl2net_set` (
  `uid` int(11) NOT NULL AUTO_INCREMENT,
  `userid` int(11) DEFAULT NULL,
  `mon_time` datetime DEFAULT NULL,
  `allow_set` bit(1) DEFAULT NULL,
  `start_set` bit(1) DEFAULT NULL,
  `start_time` datetime DEFAULT NULL,
  `end_set` bit(1) DEFAULT NULL,
  `end_time` datetime DEFAULT NULL,
  PRIMARY KEY (`uid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

#
# Data for table "tbl2net_set"
#

/*!40000 ALTER TABLE `tbl2net_set` DISABLE KEYS */;
/*!40000 ALTER TABLE `tbl2net_set` ENABLE KEYS */;

#
# Source for table "tbl2netip_mon"
#

DROP TABLE IF EXISTS `tbl2netip_mon`;
CREATE TABLE `tbl2netip_mon` (
  `uid` int(11) NOT NULL AUTO_INCREMENT,
  `userid` int(11) DEFAULT NULL,
  `mon_time` datetime DEFAULT NULL,
  `app_name` varchar(255) DEFAULT NULL,
  `app_path` varchar(255) DEFAULT NULL,
  `local_ipaddr` varchar(255) DEFAULT NULL,
  `remote_ipaddr` varchar(255) DEFAULT NULL,
  `status` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`uid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=COMPACT;

#
# Data for table "tbl2netip_mon"
#

/*!40000 ALTER TABLE `tbl2netip_mon` DISABLE KEYS */;
/*!40000 ALTER TABLE `tbl2netip_mon` ENABLE KEYS */;

#
# Source for table "tbl2netip_set"
#

DROP TABLE IF EXISTS `tbl2netip_set`;
CREATE TABLE `tbl2netip_set` (
  `uid` int(11) NOT NULL AUTO_INCREMENT,
  `userid` int(11) DEFAULT NULL,
  `mon_time` datetime DEFAULT NULL,
  `ipaddr` varchar(255) DEFAULT NULL,
  `start_set` bit(1) DEFAULT NULL,
  `ipstart_time` datetime DEFAULT NULL,
  `end_set` bit(1) DEFAULT NULL,
  `ipend_time` datetime DEFAULT NULL,
  PRIMARY KEY (`uid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

#
# Data for table "tbl2netip_set"
#

/*!40000 ALTER TABLE `tbl2netip_set` DISABLE KEYS */;
/*!40000 ALTER TABLE `tbl2netip_set` ENABLE KEYS */;

#
# Source for table "tbl3inport_set"
#

DROP TABLE IF EXISTS `tbl3inport_set`;
CREATE TABLE `tbl3inport_set` (
  `uid` int(11) NOT NULL AUTO_INCREMENT,
  `userid` int(11) DEFAULT NULL,
  `name` varchar(255) DEFAULT NULL,
  `description` varchar(255) DEFAULT NULL,
  `groupname` varchar(255) DEFAULT NULL,
  `enabled` varchar(255) DEFAULT NULL,
  `action` varchar(255) DEFAULT NULL,
  `application` varchar(255) DEFAULT NULL,
  `service` varchar(255) DEFAULT NULL,
  `protocol` varchar(255) DEFAULT NULL,
  `local_ports` varchar(255) DEFAULT NULL,
  `remote_ports` varchar(255) DEFAULT NULL,
  `icmptypes` varchar(255) DEFAULT NULL,
  `local_address` varchar(255) DEFAULT NULL,
  `remote_address` varchar(255) DEFAULT NULL,
  `profiles` varchar(255) DEFAULT NULL,
  `interfaces` varchar(255) DEFAULT NULL,
  `interface_types` varchar(255) DEFAULT NULL,
  `edge_traversal` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`uid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

#
# Data for table "tbl3inport_set"
#

/*!40000 ALTER TABLE `tbl3inport_set` DISABLE KEYS */;
/*!40000 ALTER TABLE `tbl3inport_set` ENABLE KEYS */;

#
# Source for table "tbl3outport_set"
#

DROP TABLE IF EXISTS `tbl3outport_set`;
CREATE TABLE `tbl3outport_set` (
  `uid` int(11) NOT NULL AUTO_INCREMENT,
  `userid` int(11) DEFAULT NULL,
  `name` varchar(255) DEFAULT NULL,
  `description` varchar(255) DEFAULT NULL,
  `groupname` varchar(255) DEFAULT NULL,
  `enabled` varchar(255) DEFAULT NULL,
  `action` varchar(255) DEFAULT NULL,
  `application` varchar(255) DEFAULT NULL,
  `service` varchar(255) DEFAULT NULL,
  `protocol` varchar(255) DEFAULT NULL,
  `local_ports` varchar(255) DEFAULT NULL,
  `remote_ports` varchar(255) DEFAULT NULL,
  `icmptypes` varchar(255) DEFAULT NULL,
  `local_address` varchar(255) DEFAULT NULL,
  `remote_address` varchar(255) DEFAULT NULL,
  `profiles` varchar(255) DEFAULT NULL,
  `interfaces` varchar(255) DEFAULT NULL,
  `interface_types` varchar(255) DEFAULT NULL,
  `edge_traversal` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`uid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=COMPACT;

#
# Data for table "tbl3outport_set"
#

/*!40000 ALTER TABLE `tbl3outport_set` DISABLE KEYS */;
/*!40000 ALTER TABLE `tbl3outport_set` ENABLE KEYS */;

#
# Source for table "tbl3port_mon"
#

DROP TABLE IF EXISTS `tbl3port_mon`;
CREATE TABLE `tbl3port_mon` (
  `uid` int(11) NOT NULL AUTO_INCREMENT,
  `userid` int(11) DEFAULT NULL,
  `mon_time` datetime DEFAULT NULL,
  `direction` varchar(255) DEFAULT NULL,
  `name` varchar(255) DEFAULT NULL,
  `description` varchar(255) DEFAULT NULL,
  `groupname` varchar(255) DEFAULT NULL,
  `enabled` varchar(255) DEFAULT NULL,
  `action` varchar(255) DEFAULT NULL,
  `application` varchar(255) DEFAULT NULL,
  `service` varchar(255) DEFAULT NULL,
  `protocol` varchar(255) DEFAULT NULL,
  `local_ports` varchar(255) DEFAULT NULL,
  `remote_ports` varchar(255) DEFAULT NULL,
  `icmptypes` varchar(255) DEFAULT NULL,
  `local_address` varchar(255) DEFAULT NULL,
  `remote_address` varchar(255) DEFAULT NULL,
  `profiles` varchar(255) DEFAULT NULL,
  `interfaces` varchar(255) DEFAULT NULL,
  `interface_types` varchar(255) DEFAULT NULL,
  `edge_traversal` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`uid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

#
# Data for table "tbl3port_mon"
#

/*!40000 ALTER TABLE `tbl3port_mon` DISABLE KEYS */;
/*!40000 ALTER TABLE `tbl3port_mon` ENABLE KEYS */;

#
# Source for table "tbl4realapp_mon"
#

DROP TABLE IF EXISTS `tbl4realapp_mon`;
CREATE TABLE `tbl4realapp_mon` (
  `uid` int(11) NOT NULL AUTO_INCREMENT,
  `userid` int(11) DEFAULT NULL,
  `mon_time` datetime DEFAULT NULL,
  `app_name` varchar(255) DEFAULT NULL,
  `app_status` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`uid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

#
# Data for table "tbl4realapp_mon"
#

/*!40000 ALTER TABLE `tbl4realapp_mon` DISABLE KEYS */;
/*!40000 ALTER TABLE `tbl4realapp_mon` ENABLE KEYS */;

#
# Source for table "tbl4realscreen_mon"
#

DROP TABLE IF EXISTS `tbl4realscreen_mon`;
CREATE TABLE `tbl4realscreen_mon` (
  `uid` int(11) NOT NULL AUTO_INCREMENT,
  `userid` int(11) DEFAULT NULL,
  `screenimg_path` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`uid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

#
# Data for table "tbl4realscreen_mon"
#

/*!40000 ALTER TABLE `tbl4realscreen_mon` DISABLE KEYS */;
/*!40000 ALTER TABLE `tbl4realscreen_mon` ENABLE KEYS */;

#
# Source for table "tbl5account_set"
#

DROP TABLE IF EXISTS `tbl5account_set`;
CREATE TABLE `tbl5account_set` (
  `uid` int(11) NOT NULL AUTO_INCREMENT,
  `account` varchar(255) DEFAULT NULL,
  `server_addr` varchar(255) DEFAULT NULL,
  `server_port` varchar(255) DEFAULT NULL,
  `password` varchar(255) DEFAULT NULL,
  `auth_type` int(11) DEFAULT NULL,
  `ssl_enable` bit(1) DEFAULT NULL,
  PRIMARY KEY (`uid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

#
# Data for table "tbl5account_set"
#

/*!40000 ALTER TABLE `tbl5account_set` DISABLE KEYS */;
/*!40000 ALTER TABLE `tbl5account_set` ENABLE KEYS */;

#
# Source for table "tbl5file_his"
#

DROP TABLE IF EXISTS `tbl5file_his`;
CREATE TABLE `tbl5file_his` (
  `uid` int(11) NOT NULL AUTO_INCREMENT,
  `userid` int(11) DEFAULT NULL,
  `mon_time` datetime DEFAULT NULL,
  `action_time` datetime DEFAULT NULL,
  `action` varchar(255) DEFAULT NULL,
  `file_type` varchar(255) DEFAULT NULL,
  `file_name` varchar(255) DEFAULT NULL,
  `file_path` varchar(1024) DEFAULT NULL,
  PRIMARY KEY (`uid`)
) ENGINE=InnoDB AUTO_INCREMENT=8860 DEFAULT CHARSET=utf8;

#
# Data for table "tbl5file_his"
#

/*!40000 ALTER TABLE `tbl5file_his` DISABLE KEYS */;
/*!40000 ALTER TABLE `tbl5file_his` ENABLE KEYS */;

#
# Source for table "tbl5mail_his"
#

DROP TABLE IF EXISTS `tbl5mail_his`;
CREATE TABLE `tbl5mail_his` (
  `uid` int(11) NOT NULL AUTO_INCREMENT,
  `action_time` datetime DEFAULT NULL,
  `action` varchar(255) DEFAULT NULL,
  `addr1` varchar(255) DEFAULT NULL,
  `addr2` varchar(255) DEFAULT NULL,
  `contents` varchar(1024) DEFAULT NULL,
  PRIMARY KEY (`uid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

#
# Data for table "tbl5mail_his"
#

/*!40000 ALTER TABLE `tbl5mail_his` DISABLE KEYS */;
/*!40000 ALTER TABLE `tbl5mail_his` ENABLE KEYS */;

#
# Source for table "tbl5print_his"
#

DROP TABLE IF EXISTS `tbl5print_his`;
CREATE TABLE `tbl5print_his` (
  `uid` int(11) NOT NULL AUTO_INCREMENT,
  `userid` int(11) DEFAULT NULL,
  `mon_time` datetime DEFAULT NULL,
  `print_time` datetime DEFAULT NULL,
  `print_file` varchar(255) DEFAULT NULL,
  `print_path` varchar(255) DEFAULT NULL,
  `printer` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`uid`)
) ENGINE=InnoDB AUTO_INCREMENT=18 DEFAULT CHARSET=utf8;

#
# Data for table "tbl5print_his"
#

/*!40000 ALTER TABLE `tbl5print_his` DISABLE KEYS */;
/*!40000 ALTER TABLE `tbl5print_his` ENABLE KEYS */;

#
# Source for table "tbl6comlock_set"
#

DROP TABLE IF EXISTS `tbl6comlock_set`;
CREATE TABLE `tbl6comlock_set` (
  `uid` int(11) NOT NULL AUTO_INCREMENT,
  `userid` int(11) DEFAULT NULL,
  `mouse_lock` bit(1) DEFAULT NULL,
  `keyboard_lock` bit(1) DEFAULT NULL,
  `shutdown_set` tinyint(3) DEFAULT NULL,
  PRIMARY KEY (`uid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

#
# Data for table "tbl6comlock_set"
#

/*!40000 ALTER TABLE `tbl6comlock_set` DISABLE KEYS */;
/*!40000 ALTER TABLE `tbl6comlock_set` ENABLE KEYS */;

#
# Source for table "tbl6msg_his"
#

DROP TABLE IF EXISTS `tbl6msg_his`;
CREATE TABLE `tbl6msg_his` (
  `uid` int(11) NOT NULL AUTO_INCREMENT,
  `userid` int(11) DEFAULT NULL,
  `msg_time` datetime DEFAULT NULL,
  `msg_content` varchar(1024) DEFAULT NULL,
  PRIMARY KEY (`uid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

#
# Data for table "tbl6msg_his"
#

/*!40000 ALTER TABLE `tbl6msg_his` DISABLE KEYS */;
/*!40000 ALTER TABLE `tbl6msg_his` ENABLE KEYS */;

#
# Source for table "tbl6proc_mon"
#

DROP TABLE IF EXISTS `tbl6proc_mon`;
CREATE TABLE `tbl6proc_mon` (
  `uid` int(11) NOT NULL AUTO_INCREMENT,
  `userid` int(11) DEFAULT NULL,
  `mon_time` datetime DEFAULT NULL,
  `sid` int(11) DEFAULT NULL,
  `pid` int(11) DEFAULT NULL,
  `proc_name` varchar(255) DEFAULT NULL,
  `proc_path` varchar(255) DEFAULT NULL,
  `start_time` datetime DEFAULT NULL,
  PRIMARY KEY (`uid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

#
# Data for table "tbl6proc_mon"
#

/*!40000 ALTER TABLE `tbl6proc_mon` DISABLE KEYS */;
/*!40000 ALTER TABLE `tbl6proc_mon` ENABLE KEYS */;

#
# Source for table "tbl6remote_set"
#

DROP TABLE IF EXISTS `tbl6remote_set`;
CREATE TABLE `tbl6remote_set` (
  `uid` int(11) NOT NULL AUTO_INCREMENT,
  `userid` int(11) DEFAULT NULL,
  `remote_date` date DEFAULT NULL,
  `remote_time` time DEFAULT NULL,
  `remote_period` time DEFAULT NULL,
  PRIMARY KEY (`uid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

#
# Data for table "tbl6remote_set"
#

/*!40000 ALTER TABLE `tbl6remote_set` DISABLE KEYS */;
/*!40000 ALTER TABLE `tbl6remote_set` ENABLE KEYS */;

#
# Source for table "tbl6user_set"
#

DROP TABLE IF EXISTS `tbl6user_set`;
CREATE TABLE `tbl6user_set` (
  `uid` int(11) NOT NULL AUTO_INCREMENT,
  `userid` int(11) DEFAULT NULL,
  `mon_time` datetime DEFAULT NULL,
  `nType` int(11) DEFAULT NULL,
  `user_name` varchar(255) DEFAULT NULL,
  `privilege` varchar(255) DEFAULT NULL,
  `password` varchar(255) DEFAULT NULL,
  `logon_time` datetime DEFAULT NULL,
  `logoff_time` datetime DEFAULT NULL,
  `logon_state` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`uid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

#
# Data for table "tbl6user_set"
#

/*!40000 ALTER TABLE `tbl6user_set` DISABLE KEYS */;
/*!40000 ALTER TABLE `tbl6user_set` ENABLE KEYS */;

#
# Source for table "tbl7netapp_his"
#

DROP TABLE IF EXISTS `tbl7netapp_his`;
CREATE TABLE `tbl7netapp_his` (
  `uid` int(11) NOT NULL AUTO_INCREMENT,
  `userid` int(11) DEFAULT NULL,
  `mon_time` datetime DEFAULT NULL,
  `access_time` datetime DEFAULT NULL,
  `app_name` varchar(255) DEFAULT NULL,
  `app_path` varchar(255) DEFAULT NULL,
  `local_addr` varchar(255) DEFAULT NULL,
  `remote_addr` varchar(255) DEFAULT NULL,
  `status` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`uid`)
) ENGINE=InnoDB AUTO_INCREMENT=3156 DEFAULT CHARSET=utf8;

#
# Data for table "tbl7netapp_his"
#

/*!40000 ALTER TABLE `tbl7netapp_his` DISABLE KEYS */;
/*!40000 ALTER TABLE `tbl7netapp_his` ENABLE KEYS */;

#
# Source for table "tbl7web_his"
#

DROP TABLE IF EXISTS `tbl7web_his`;
CREATE TABLE `tbl7web_his` (
  `uid` int(11) NOT NULL AUTO_INCREMENT,
  `userid` int(11) DEFAULT NULL,
  `mon_time` datetime DEFAULT NULL,
  `access_time` datetime DEFAULT NULL,
  `url_addr` varchar(1024) DEFAULT NULL,
  `title_name` varchar(512) DEFAULT NULL,
  PRIMARY KEY (`uid`)
) ENGINE=InnoDB AUTO_INCREMENT=156 DEFAULT CHARSET=utf8;

#
# Data for table "tbl7web_his"
#

/*!40000 ALTER TABLE `tbl7web_his` DISABLE KEYS */;
/*!40000 ALTER TABLE `tbl7web_his` ENABLE KEYS */;

#
# Source for table "tbl8change_his"
#

DROP TABLE IF EXISTS `tbl8change_his`;
CREATE TABLE `tbl8change_his` (
  `uid` int(11) NOT NULL AUTO_INCREMENT,
  `userid` int(11) DEFAULT NULL,
  PRIMARY KEY (`uid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

#
# Data for table "tbl8change_his"
#

/*!40000 ALTER TABLE `tbl8change_his` DISABLE KEYS */;
/*!40000 ALTER TABLE `tbl8change_his` ENABLE KEYS */;

#
# Source for table "tbl8instapp_his"
#

DROP TABLE IF EXISTS `tbl8instapp_his`;
CREATE TABLE `tbl8instapp_his` (
  `uid` int(11) NOT NULL AUTO_INCREMENT,
  `userid` int(11) DEFAULT NULL,
  `mon_time` datetime DEFAULT NULL,
  `app_name` varchar(255) DEFAULT NULL,
  `app_version` varchar(255) DEFAULT NULL,
  `app_path` varchar(255) DEFAULT NULL,
  `inst_time` datetime DEFAULT NULL,
  PRIMARY KEY (`uid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

#
# Data for table "tbl8instapp_his"
#

/*!40000 ALTER TABLE `tbl8instapp_his` DISABLE KEYS */;
/*!40000 ALTER TABLE `tbl8instapp_his` ENABLE KEYS */;

#
# Source for table "tbl8proc_mon"
#

DROP TABLE IF EXISTS `tbl8proc_mon`;
CREATE TABLE `tbl8proc_mon` (
  `uid` int(11) NOT NULL AUTO_INCREMENT,
  `userid` int(11) DEFAULT NULL,
  `mon_time` datetime DEFAULT NULL,
  `sid` int(11) DEFAULT NULL,
  `pid` int(11) DEFAULT NULL,
  `proc_name` varchar(255) DEFAULT NULL,
  `proc_path` varchar(255) DEFAULT NULL,
  `mem_usage` varchar(255) DEFAULT NULL,
  `cpu_usage` varchar(255) DEFAULT NULL,
  `start_time` datetime DEFAULT NULL,
  PRIMARY KEY (`uid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

#
# Data for table "tbl8proc_mon"
#

/*!40000 ALTER TABLE `tbl8proc_mon` DISABLE KEYS */;
/*!40000 ALTER TABLE `tbl8proc_mon` ENABLE KEYS */;

#
# Source for table "tblenviron_list"
#

DROP TABLE IF EXISTS `tblenviron_list`;
CREATE TABLE `tblenviron_list` (
  `uid` int(11) NOT NULL AUTO_INCREMENT,
  `adminid` binary(255) DEFAULT NULL,
  `func_disable` varchar(12) DEFAULT NULL,
  PRIMARY KEY (`uid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

#
# Data for table "tblenviron_list"
#

/*!40000 ALTER TABLE `tblenviron_list` DISABLE KEYS */;
/*!40000 ALTER TABLE `tblenviron_list` ENABLE KEYS */;

#
# Source for table "tblkey_list"
#

DROP TABLE IF EXISTS `tblkey_list`;
CREATE TABLE `tblkey_list` (
  `uid` int(11) NOT NULL AUTO_INCREMENT,
  `hwinfo` varchar(255) DEFAULT NULL,
  `serial` varchar(255) DEFAULT NULL,
  `activate` varchar(1024) DEFAULT NULL,
  `usbkey` varchar(255) DEFAULT NULL,
  `status` varchar(2048) DEFAULT NULL,
  PRIMARY KEY (`uid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

#
# Data for table "tblkey_list"
#

/*!40000 ALTER TABLE `tblkey_list` DISABLE KEYS */;
/*!40000 ALTER TABLE `tblkey_list` ENABLE KEYS */;

#
# Source for table "tbltimestamp_set"
#

DROP TABLE IF EXISTS `tbltimestamp_set`;
CREATE TABLE `tbltimestamp_set` (
  `uid` int(11) NOT NULL AUTO_INCREMENT,
  `userid` int(11) DEFAULT NULL,
  `tbl1appdisableset` datetime DEFAULT NULL,
  `tbl1prohibitset` datetime DEFAULT NULL,
  `tbl2netset` datetime DEFAULT NULL,
  `tbl2netipmon` datetime DEFAULT NULL,
  `tbl3inportset` datetime DEFAULT NULL,
  `tbl3outportset` datetime DEFAULT NULL,
  `tbl3portmon` datetime DEFAULT NULL,
  `tbl4realappmon` datetime DEFAULT NULL,
  `tbl4realscreenmon` datetime DEFAULT NULL,
  `tbl5filehis` datetime DEFAULT NULL,
  `tbl5printhis` datetime DEFAULT NULL,
  `tbl5mailhis` datetime DEFAULT NULL,
  `tbl6comlockset` datetime DEFAULT NULL,
  `tbl6msghis` datetime DEFAULT NULL,
  `tbl6procmon` datetime DEFAULT NULL,
  `tbl6remoteset` datetime DEFAULT NULL,
  `tbl6userset` datetime DEFAULT NULL,
  `tbl7netapphis` datetime DEFAULT NULL,
  `tbl7webhis` datetime DEFAULT NULL,
  `tbl8changehis` datetime DEFAULT NULL,
  `tbl8instapphis` datetime DEFAULT NULL,
  `tbl8procmon` datetime DEFAULT NULL,
  `tbl8devicemon` datetime DEFAULT NULL,
  PRIMARY KEY (`uid`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

#
# Data for table "tbltimestamp_set"
#

/*!40000 ALTER TABLE `tbltimestamp_set` DISABLE KEYS */;
/*!40000 ALTER TABLE `tbltimestamp_set` ENABLE KEYS */;

#
# Source for table "tbluser_list"
#

DROP TABLE IF EXISTS `tbluser_list`;
CREATE TABLE `tbluser_list` (
  `uid` int(11) NOT NULL AUTO_INCREMENT,
  `userid` int(11) DEFAULT NULL,
  `macaddr` varchar(40) DEFAULT NULL,
  `ipaddr` varchar(40) DEFAULT NULL,
  `username` varchar(40) DEFAULT NULL,
  `machine` varchar(80) DEFAULT NULL,
  `logon` bit(1) DEFAULT NULL,
  PRIMARY KEY (`uid`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8;

#
# Data for table "tbluser_list"
#

/*!40000 ALTER TABLE `tbluser_list` DISABLE KEYS */;
/*!40000 ALTER TABLE `tbluser_list` ENABLE KEYS */;

/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;
/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
