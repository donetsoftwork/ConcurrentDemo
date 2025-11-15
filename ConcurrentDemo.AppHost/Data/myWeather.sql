-- MySql init script
USE `myWeather`;
CREATE TABLE IF NOT EXISTS `weathers`
(
    `Date` Date NOT NULL,
    `Summary` varchar(255) NOT NULL,
    `TemperatureC` int NOT NULL,
    PRIMARY KEY (`Date`)
);