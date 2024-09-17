Integrate all available metrics from AIDA64 into MoBro by utilizing [AIDA64](https://www.aida64.com/)'s shared memory
support.

# Disclaimer

This plugin is developed and provided by ModBros and is not affiliated with AIDA64.  
It utilizes AIDA64's shared memory interface to share metrics with third-party applications.

# Setup

To use this plugin, ensure you have **AIDA64 running with 'Shared Memory' enabled**.

As long as AIDA64 is running in the background, the plugin will automatically pick up all available metrics, making them
available in MoBro.  
No further configuration required.

## How to Enable 'Shared Memory'

1. Open the AIDA64 preferences
2. Select 'External Applications on the left'
3. Enable 'Enable shared memory'

# Settings

This plugin exposes the following settings:

| Setting          | Default | Explanation                                                                                                                                                         |
|------------------|---------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Update frequency | 1000 ms | The frequency (in milliseconds) at which to read and update metrics from shared memory. Lower values will update metrics more frequently but may increase CPU load. |
