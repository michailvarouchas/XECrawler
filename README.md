# XECrawler

##Add new file settings.config to the solution

###<appSettings>

  ###<add key="numberOfPages" value="1" />

  ###<add key="xeUsername" value="" />
  ###<<add key="xePassword" value="" />

  ###<<!--ADDING PATH FOR HTML PAGE (EMAIL)-->
  ###<<add key="templatePath" value="C:\Users\..." />
  
  ###<<add key="sendGridKey" value="" />
  ###<<add key="mailFrom" value="info@XECrawler.gr;XECrawler" />

  ###<<!--ADDING NEW EMAIL RECIPIENT WITH NAME-->
  ###<<add key="mailTo" value="dummy1@gmail.com;dummy2@yahoo.gr" />
  ###<<add key ="mailToName" value="dummy1;dummy2" />

  ###<<!--ADDING FILEPATH FOR EXCEL EXPORT-->
  ###<<add key="filePath" value="C:\Users\musse\source\repos\XECrawler" />
  
  ###<<!--ADDING PROPERTY BASE URL AND SEARCH PAGES BASE URL-->
  ###<<add key="propertyDetailsPageUrl" value="link" />
  ###<<add key="propertySearchPageUrls" value="link1 | link2 | link3" />

  ###<<add key="ClientSettingsProvider.ServiceUri" value="" />
###<</appSettings>
