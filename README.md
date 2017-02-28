NLog.Splunk
===================

Splunk target to send logs in splunk

----------


Getting Started
-------------

Use the GUI or the following command in the Package Manager Console

    Install-Package NLog.Splunk


----------

Configuration
-------------

For async configuration  you can use with BufferingWrapper

    <target xsi:type="BufferingWrapper" 
    name="f" 
    bufferSize="100" 
    slidingTimeout="true" 
    flushTimeout="10000" >
	    <target xsi:type="Splunk"               
        layout="${longdate} | ${uppercase:${level}} | ${message} | ${exception}"
        host="{url-to-splunk}"
        username="{your-username}"
        password="{your-password}"
        index="{your-index}"
        source="{your-source}"
        sourceType="{your-source-type}"
        />
    </target>

Otherwise you can use in normal way

    <target xsi:type="Splunk" 
    name="f"             
    layout="${longdate} | ${uppercase:${level}} | ${message} | ${exception}"
    host="{url-to-splunk}"
    username="{your-username}"
    password="{your-password}"
    index="{your-index}"
    source="{your-source}"
    sourceType="{your-source-type}"
    />

Don't forget to add your rules

    <logger name="*" minlevel="Info" writeTo="f" />

![enter image description here](http://blogs.splunk.com/wp-content/uploads/2015/12/splunk_logging_driver_advanced.png)
