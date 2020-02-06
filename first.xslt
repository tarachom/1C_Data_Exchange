<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="html" indent="yes"/>

  <xsl:template match="/">

    <html>
      <head>
        <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
      </head>
      <body>

        <table>

          <xsl:for-each select="root/row">

            <tr>
              <td>
                <xsl:value-of select="name"/>
              </td>
              <td>
                <xsl:value-of select="code"/>
              </td>
              <td>
                <xsl:value-of select="desc"/>
              </td>
            </tr>

          </xsl:for-each>

        </table>

      </body>
    </html>

  </xsl:template>

</xsl:stylesheet>
