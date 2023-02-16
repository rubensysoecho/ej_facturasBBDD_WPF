
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
using System.Windows.Shapes;

namespace EjRepasoWPF
{


    public partial class VerFacturas : Window
    {

        BDMySql bbdd = BDMySql.getInstance();

        public interface iVerFacturas
        {
            void recibeIDFactura(int id_factura);

        }

 
        iVerFacturas listener;

        public VerFacturas(iVerFacturas listener)
        {
            InitializeComponent();
            this.listener = listener;

            // Completar: Obtener todas las facturas y visualizar en el dataGrid
            DataTable datos = bbdd.LanzaSelect("SELECT * FROM `facturas`", true);

            dataGrid_facturas.ItemsSource = datos.DefaultView;
            dataGrid_facturas.AutoGenerateColumns = true;
            dataGrid_facturas.CanUserAddRows = false;


        }

        private void b_Cancelar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void b_Aceptar_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid_facturas.SelectedIndex >= 0)
            {

                DataRowView fila = (DataRowView)dataGrid_facturas.SelectedItem;
                int id = Int32.Parse(fila[0].ToString()); // fila[0] debe ser el ID de la factura
                if (listener!=null)
                {
                    Close();
                    listener.recibeIDFactura(id);
                }


            }
            else {
                MessageBox.Show("Debe seleccionar una fila", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);

            }
        }
    }
}
