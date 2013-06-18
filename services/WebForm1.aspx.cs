using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Security.Cryptography;
using RSA;

namespace services
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

  
            
        }

        void oRSA_OnKeysGenerated(object sender)
        {
            RSACrypto oRSA = (RSACrypto)sender;
        }
    }
}