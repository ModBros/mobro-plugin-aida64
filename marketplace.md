Integrate all AIDA64 metrics into MoBro through [AIDA64](https://www.aida64.com/)'s shared memory functionality.

# Disclaimer

This plugin is developed and provided by ModBros and is not associated with AIDA64.  
It uses AIDA64's shared memory interface to transfer metrics to third-party applications.

---

# Setup

To use this plugin, make sure **AIDA64 is running with 'Shared Memory' enabled**.  
With AIDA64 active in the background, the plugin will automatically detect and bring all available metrics into MoBro.  
No additional configuration is necessary.

## How to Enable 'Shared Memory'

1. Open AIDA64 preferences.
2. Navigate to the 'External Applications' section on the left.
3. Check the box for 'Enable shared memory'.

---

# Settings

This plugin offers the following configurable settings:

| Setting          | Default | Description                                                                                                                                                   |
|------------------|---------|---------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Update Frequency | 1000 ms | The frequency (in milliseconds) for reading and updating metrics from shared memory. Lower values result in more frequent updates but may increase CPU usage. |
