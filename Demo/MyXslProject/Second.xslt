<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
>
  <xsl:import href="First.xslt"/>

  <xsl:output method="xml" indent="yes"/>

  <xsl:include href="First.xslt"/>

  <xsl:template match="@* | node()">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()"/>

      <xsl:apply-templates mode="Things" />
    </xsl:copy>
  </xsl:template>


  <xsl:template mode="Things" match="MyThings">

  </xsl:template>
</xsl:stylesheet>
