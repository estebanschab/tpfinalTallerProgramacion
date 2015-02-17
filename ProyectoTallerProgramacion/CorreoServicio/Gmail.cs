﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataTransferObject;
using System.Net.Mail;
using OpenPop.Pop3;

namespace CorreoServicio
{
    /// <summary>
    /// Clase utilizada para relacionar el programa con el servicio de correo correspondiente a Gmail.
    /// </summary>
    public class Gmail : ServicioCorreo
    {
        /// <summary>
        /// Constructor de la clase. Llama al constructor de la superclase pasandole como parametro
        /// el nombre del Servicio de Correo.
        /// </summary>
        public Gmail() : base("Gmail") {}

        /// <summary>
        /// Metodo utilizado para enviar un correo.
        /// </summary>
        /// <param name="pCorreo">Correo a ser enviado.</param>
        public override void EnviarCorreo(CorreoDTO pCorreo)
        {
            MailMessage correo = new MailMessage();
            
            // Ver si ponemos iDireccion o pCorreo.CuentaOrigen

            correo.From = new MailAddress(this.Direccion);
            correo.To.Add(pCorreo.CuentaDestino);
            correo.Subject = pCorreo.Asunto;
            correo.Body = pCorreo.Texto;            
            if (pCorreo.Adjuntos != null) 
            {
                foreach (string archivo in pCorreo.Adjuntos)
                {
                    Attachment attach = new Attachment(@archivo);
                    correo.Attachments.Add(attach);
                }
            }
            SmtpClient cliente = new SmtpClient("smtp.gmail.com");
            cliente.EnableSsl = true;
            cliente.Port = 587;
            cliente.Credentials = new System.Net.NetworkCredential(this.Direccion,this.Contraseña);

            //Aca tendriamos que poner un try-catch
            cliente.Send(correo);
        }

        /// <summary>
        /// Metodo que descarga los correos del Servicio de correo que pertenecen a la cuenta <paramref name="pCuenta"/>.
        /// </summary>
        /// <param name="pCuenta">Cuenta de la cual se descargan los correos.</param>
        /// <returns>Retorna una lista de correos.</returns>
        public override IList<CorreoDTO> DescargarCorreos(CuentaDTO pCuenta)
        {
            Pop3Client pop = new Pop3Client();
            pop.Connect("pop.gmail.com", 995, true);
            pop.Authenticate(pCuenta.Direccion, pCuenta.Contraseña);
            int cantidadMensajes = pop.GetMessageCount();
            List<CorreoDTO> mCorreos = new List<CorreoDTO>();
            OpenPop.Mime.Message mensaje;
            
            for (int i = cantidadMensajes; i > 0; i--)      
            {
                mensaje = pop.GetMessage(i);
                CorreoDTO correo = new CorreoDTO();
                mCorreos.Add(new CorreoDTO()
                {
                    Fecha = Convert.ToDateTime(mensaje.Headers.DateSent),
                    TipoCorreo = "Recibido",
                    Texto = mensaje.ToMailMessage().Body,                    
                    CuentaOrigen = mensaje.Headers.From.Address,
                    CuentaDestino = pCuenta.Direccion,    // Reemplazar esto!!!
                    Asunto = mensaje.Headers.Subject,
                    Leido = 0,
                    ServicioId = mensaje.Headers.MessageId
                });
            }
            return mCorreos;
        }
  
    }
}
