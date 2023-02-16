
using EjemploConexiónBDAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EjRepasoWPF
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window, VerFacturas.iVerFacturas
    {
        public const decimal IVA = 0.21m;

        public BDMySql bd = BDMySql.getInstance();

        // cajas de texto
        public String Dni { get; private set; }
        public String Nombre { get; private set; }
        public String Apellidos { get; private set; }

        // DatePickers
        public DateTime? FechaLLegada { get; private set; }
        public String StrFechaLLegada => FechaLLegada?.ToString("yyyy-MM-dd");

        public DateTime? FechaSalida { get; private set; }
        public String StrFechaSalida => FechaSalida?.ToString("yyyy-MM-dd");

        // RadioButtons
        public enum EnumTipoHabitacion { Estándar, Superior }
        EnumTipoHabitacion TipoHabitacion { get; set; }


        // Checkboxes
        private short _buffet;
        public short Buffet
        {
            get
            {
                return _buffet;
            }

            private set {
                if (value!=0 && value!=1)
                {
                    throw new Exception("Sólo se admiten como valores 1 y 0");
                }
                else
                {
                    _buffet = value;
                }
            }
        }

        private short _wifi;
        public short Wifi
        {
            get
            {
                return _wifi; ;
            }

            private set
            {
                if (value != 0 && value != 1)
                {
                    throw new Exception("Sólo se admiten como valores 1 y 0");
                }
                else
                {
                    _wifi = value;
                }
            }
        }


        private short _spa;
        public short Spa
        {
            get
            {
                return _spa; ;
            }

            private set
            {
                if (value != 0 && value != 1)
                {
                    throw new Exception("Sólo se admiten como valores 1 y 0");
                }
                else
                {
                    _spa = value;
                }
            }
        }


        // Datos Calculados
        public int Noches { get; private set; }
        public decimal Precio_habitacion { get; private set; }
        public decimal Precio_servicios { get; private set; }
        public decimal Importe_servicios { get; private set; }
        public decimal Importe_hospedaje { get; private set; }
        public decimal Importe_total     { get; private set; }
        public decimal Importe_IVA     { get; private set; }
        public decimal Total_pagar       { get; private set; }



        bool chequeaDNI(String dni)
        {
            
            //Comprobamos si el DNI tiene 9 digitos
            if (dni.Length != 9){
                //No es un DNI Valido
                return false;
            }


            //Extraemos los números y la letra
            string dniNumeros = dni.Substring(0, dni.Length - 1);
            string dniLetra = dni.Substring(dni.Length - 1, 1);
            //Intentamos convertir los números del DNI a integer
            int numeros = -1;
            try
            {
                numeros = Int32.Parse(dniNumeros);
            }
            catch
            {
                return false;
            }

            string[] control = { "T", "R", "W", "A", "G", "M", "Y", "F", "P", "D", "X", "B", "N", "J", "Z", "S", "Q", "V", "H", "L", "C", "K", "E" };
            if (control[numeros % 23] != dniLetra){
                //La letra del DNI es incorrecta
                return false;
            }

            //DNI Valido 
            return true;

        }


        void limpiar()
        {
            lbl_ID.Content = "";
            tb_dni.Text = "";
            tb_apellidos.Text = "";
            tb_nombre.Text = "";

            dp_llegada.SelectedDate = null;
            dp_salida.SelectedDate = null;
            lbl_noches.Content = "";

            rb_estandar.IsChecked = false;
            rb_superior.IsChecked = false;

            chk_desayuno.IsChecked = false;
            chk_spa.IsChecked = false;
            chk_wifi.IsChecked = false;

            tb_importe_hospedaje.Text = "";
            tb_importe_servicios.Text = "";
            tb_importe_total.Text = "";
            tb_iva.Text = "";
            tb_total_pagar.Text = "";

            Dni = "";
            Nombre = "";
            Apellidos = "";
            FechaLLegada = null;
            FechaSalida = null;
            TipoHabitacion = EnumTipoHabitacion.Estándar;
            Buffet = 0;
            Spa = 0;
            Wifi = 0;
            Importe_hospedaje = 0;
            Importe_servicios = 0;
            Importe_IVA = 0;
            Importe_total = 0;

            b_calcular.IsEnabled = true;

            tb_dni.IsReadOnly = false;
            tb_nombre.IsReadOnly = false;
            tb_apellidos.IsReadOnly = false;
            dp_llegada.IsEnabled = true;
            dp_salida.IsEnabled = true;
            groupBoxServicios.IsEnabled = true;
            groupBoxHabitacion.IsEnabled = true;
            tb_importe_hospedaje.IsReadOnly = false;
            tb_importe_servicios.IsReadOnly = false;
            tb_importe_total.IsReadOnly = false;
            tb_iva.IsReadOnly = false;
            tb_importe_total.IsReadOnly = false;
        }

        public MainWindow()
        {
            InitializeComponent();
            b_guardar.IsEnabled = false;
            tb_dni.Focus();

            // Completar: Conectarse y abri conexión con la base de datos
        }

        private void b_limpiar_Click(object sender, RoutedEventArgs e)
        {
            limpiar();

        }

        private void b_Salir_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void b_calcular_Click(object sender, RoutedEventArgs e)
        {
            if (tb_dni.Text.Length == 0 || !chequeaDNI(tb_dni.Text))
            {
                MessageBox.Show("Debe introducir un DNI válido ", "Aviso");
                tb_dni.Focus();
                return;
            }
            else
            {
                Dni = tb_dni.Text;
            }

            if (tb_apellidos.Text.Length == 0)
            {
                MessageBox.Show("Debe introducir los apellidos del cliente", "Aviso");
                tb_apellidos.Focus();
                return;
            }
            else
            {
                Apellidos = tb_apellidos.Text;
            }

            if (tb_nombre.Text.Length == 0)
            {
                MessageBox.Show("Debe  introducir el nombre cliente", "Aviso");
                tb_nombre.Focus();
                return;
            }
            else
            {
                Nombre = tb_nombre.Text;
            }


            if (dp_salida.SelectedDate == null || dp_llegada.SelectedDate == null || dp_salida.SelectedDate <= dp_llegada.SelectedDate
                || (lbl_ID.Content.ToString().Length==0 && dp_llegada.SelectedDate < DateTime.Today))
            {
                MessageBox.Show("Debe seleccionar intervalo de fechas correcto", "Aviso");
                return;
            }
            else
            {
                TimeSpan intervalo = (TimeSpan)(dp_salida.SelectedDate - dp_llegada.SelectedDate);

                Noches = intervalo.Days;


                lbl_noches.Content = Noches.ToString();

                FechaLLegada = dp_llegada.SelectedDate;
                FechaSalida = dp_salida.SelectedDate;
            }

            if (rb_estandar.IsChecked == false && rb_superior.IsChecked == false)
            {
                MessageBox.Show("Debe seleccionar el tipo de habitación", "Aviso");
                return;
            }
            else
            {
                Precio_habitacion = 0;
                if (rb_estandar.IsChecked == true) { 
                    Precio_habitacion = 60;
                    TipoHabitacion = EnumTipoHabitacion.Estándar;
                }
                else if (rb_superior.IsChecked == true) {
                    TipoHabitacion = EnumTipoHabitacion.Superior;
                    Precio_habitacion = 120;
                }
            }


            Precio_servicios = 0;
          
            if (chk_desayuno.IsChecked == true)
            {
                Buffet = 1;
                Precio_servicios += 15;
            }
            else
            {
                Buffet = 0;
            }

            if (chk_wifi.IsChecked == true)
            {
                Wifi = 1;
                Precio_servicios += 5;
            }
            else
            {
                Wifi = 0;
            }

            if (chk_spa.IsChecked == true)
            {
                Spa = 1;
                Precio_servicios += 30;
            }
            else
            {
                Spa = 0;
            }

            Importe_hospedaje = Precio_habitacion * Noches;
            Importe_servicios = Precio_servicios * Noches;
            Importe_total = Importe_hospedaje + Importe_servicios;
            Importe_IVA = Importe_total * IVA;
            Total_pagar = Importe_total + Importe_IVA;

            tb_importe_hospedaje.Text = string.Format("{0:c}", Importe_hospedaje);
            tb_importe_servicios.Text = string.Format("{0:c}", Importe_servicios);
            tb_importe_total.Text = string.Format("{0:c}",  Importe_total);
            tb_iva.Text = string.Format("{0:c}", Importe_IVA); 
            tb_total_pagar.Text = string.Format("{0:c}", Total_pagar);

            b_guardar.IsEnabled = true;
        }

        private void b_guardar_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("¿Está seguro que quiere crear una nueva factura?","Atención",MessageBoxButton.YesNo,MessageBoxImage.Warning)==MessageBoxResult.Yes) {
                int filasInsertadas=-1;

                // Completar: Lanzar sentencia Insert para insertar la factura introducida por el usuario
                filasInsertadas =  bd.EjecutaConsultaAccion($"INSERT INTO `facturas`(`dni`, `nombre`, `apellidos`," +
                    $" `llegada`, `salida`, `habitacion`, `buffet`, `wifi`, `spa`) VALUES ('{Dni}','{Nombre}','{Apellidos}'," +
                    $"'{StrFechaLLegada}','{StrFechaSalida}','{TipoHabitacion}',{Buffet},{Wifi},{Spa})");

                if (filasInsertadas == 1)
                {
                    MessageBox.Show("Factura guardada", "Operación correcta", MessageBoxButton.OK, MessageBoxImage.Information);
                    limpiar();
                    b_guardar.IsEnabled = false;
                }
                else
                {
                    MessageBox.Show("Problema al guardar la factura", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            

        }

        private void b_verFacturas_Click(object sender, RoutedEventArgs e)
        {
            VerFacturas ver_facturas = new VerFacturas(this);
            ver_facturas.ShowDialog();
        }

        public void recibeIDFactura(int id_factura)
        {
            ponDatos(id_factura);
        }

        void ponDatos(int id_factura)
        {
            DataTable datosFactura = null;

            // Completar: Lazar sentencia SELECT PARA obtener los datos de la factura

            if (id_factura != 0)
            {
                datosFactura = bd.LanzaSelect($"SELECT * FROM `facturas` WHERE ID = {id_factura}", true);
            }
            else
            {
                datosFactura = bd.LanzaSelect("SELECT * FROM `facturas`", true);
            }
           

            if (datosFactura.Rows.Count>0)
            {
                lbl_ID.Content = datosFactura.Rows[0]["id"].ToString();
                tb_dni.Text = datosFactura.Rows[0]["dni"].ToString();
                tb_apellidos.Text = datosFactura.Rows[0]["apellidos"].ToString(); 
                tb_nombre.Text = datosFactura.Rows[0]["nombre"].ToString(); 

                dp_llegada.SelectedDate = DateTime.Parse(datosFactura.Rows[0]["llegada"].ToString());
                dp_salida.SelectedDate = DateTime.Parse(datosFactura.Rows[0]["salida"].ToString());
                lbl_noches.Content = "";

                EnumTipoHabitacion tipoHabitacion = (EnumTipoHabitacion)Enum.Parse(typeof(EnumTipoHabitacion), datosFactura.Rows[0]["habitacion"].ToString());
                if (tipoHabitacion == EnumTipoHabitacion.Estándar)
                {
                    rb_estandar.IsChecked = true;
                }
                else
                {

                    rb_superior.IsChecked = true;
                }

                chk_desayuno.IsChecked = bool.Parse(datosFactura.Rows[0]["buffet"].ToString());
                chk_spa.IsChecked = bool.Parse(datosFactura.Rows[0]["spa"].ToString()); ;
                chk_wifi.IsChecked = bool.Parse(datosFactura.Rows[0]["wifi"].ToString());
                
                b_calcular.IsEnabled = true;
                b_calcular_Click(null,null);
                b_calcular.IsEnabled = false;
                b_guardar.IsEnabled = false;

                tb_dni.IsReadOnly = true;
                tb_nombre.IsReadOnly = true;
                tb_apellidos.IsReadOnly = true;
                dp_llegada.IsEnabled = false;
                dp_salida.IsEnabled = false;
                groupBoxServicios.IsEnabled = false;
                groupBoxHabitacion.IsEnabled = false;
                tb_importe_hospedaje.IsReadOnly = true;
                tb_importe_servicios.IsReadOnly = true;
                tb_importe_total.IsReadOnly = true;
                tb_iva.IsReadOnly = true;
                tb_importe_total.IsReadOnly = true;
            }

        }
    }
}
