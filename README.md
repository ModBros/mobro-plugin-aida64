# mobro-plugin-aida64

![GitHub tag (latest by date)](https://img.shields.io/github/v/tag/ModBros/mobro-plugin-aida64?label=version)
![GitHub](https://img.shields.io/github/license/ModBros/mobro-plugin-aida64)
[![MoBro](https://img.shields.io/badge/-MoBro-red.svg)](https://mobro.app)
[![Discord](https://img.shields.io/discord/620204412706750466.svg?color=7389D8&labelColor=6A7EC2&logo=discord&logoColor=ffffff&style=flat-square)](https://discord.com/invite/DSNX4ds)

**Aida64 plugin for MoBro**

Integrate all AIDA64 metrics into [MoBro](https://mobro.app) through [AIDA64](https://www.aida64.com/)'s shared memory
functionality.

This plugin is developed and provided by ModBros and is not associated with AIDA64.  
It uses AIDA64's shared memory interface to transfer metrics to third-party applications.

## Getting Started

To use this plugin, make sure **AIDA64 is running with 'Shared Memory' enabled**.  
With AIDA64 active in the background, the plugin will automatically detect and bring all available metrics into MoBro.  
No additional configuration is necessary.

### How to Enable 'Shared Memory'

1. Open AIDA64 preferences.
2. Navigate to the 'External Applications' section on the left.
3. Check the box for 'Enable shared memory'.

## Settings

This plugin offers the following configurable settings:

| Setting          | Default | Description                                                                                                                                                   |
|------------------|---------|---------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Update Frequency | 1000 ms | The frequency (in milliseconds) for reading and updating metrics from shared memory. Lower values result in more frequent updates but may increase CPU usage. |

## SDK

This plugin is built using the [MoBro Plugin SDK](https://github.com/ModBros/mobro-plugin-sdk).  
Developer documentation is available at [developer.mobro.app](https://developer.mobro.app).
