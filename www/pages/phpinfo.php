<?php
	if(isset($_SESSION['user_id'])){ 
?>
<div class="main">
 <table border="0" width="100%">
  <tbody>
    <tr>
      <td align="center" valign="middle">

	<!-- TITRE -->
        <table border="0" width="100%">
            <tr>
              <td class="titre" align="left"><img src="images/_divers.gif" width="32" height="32" border="0"> PHPINFO </td>
              <td align="right"> </td>
            </tr>
        </table>

	<!-- CONTENU -->
        <table width="92%" class="adminform">
          <tbody>
            <tr>
              <td width="100%" align="left" valign="top">
		<table border="0" cellspacing="0" cellpadding="0" width="100%">
		  <tr><td><font size=2>
			<?php phpinfo(); ?>
		  </font></td></tr>
		</table>
 	      </td>
            </tr>
          </tbody>
        </table>
        
      </td>
    </tr>
  </tbody>
 </table>
</div>
<?php
	}else{
		header("location:../index.php");
	}
?>
