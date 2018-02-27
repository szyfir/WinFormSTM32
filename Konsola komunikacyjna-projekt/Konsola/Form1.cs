using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.Windows.Forms.DataVisualization.Charting;
using System.Threading;

namespace PROBA
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            MozliwyPort();
            portCOM.DataReceived += serialPort_DataReceived;           
        }
        //########################################################### INICJALIZACJA POŁĄCZENIA #######################################################################################
        void MozliwyPort()
        {
            foreach (String s in SerialPort.GetPortNames()) this.cbNazwy.Items.Add(s);
            foreach (String s in Enum.GetNames(typeof(Parity))) this.cbParzystosc.Items.Add(s);         //komenda foreach pokazuje mozliwe, dostepne opcje parzystosci i bitow stopu msdn/forbot
            foreach (String s in Enum.GetNames(typeof(StopBits))) this.cbStop.Items.Add(s);            
        }
        private void bOtworz_Click_1(object sender, EventArgs e)                                         //otwarcie mozliwego portu
        {
            {
                if (cbNazwy.Text == "" || cbPredkosc.Text == "" || cbParzystosc.Text == "" || cbDane.Text == "" || cbStop.Text == "") //jezeli jakiekolwiek z ustawien portu jest puste nie ustanawia polaczenia
                {
                    rtbPolaczenie.Text = "Wybierz ustawienia portu!";
                    pbStatus.BackColor = System.Drawing.Color.Red;
                }
                else
                {
                    portCOM.PortName = cbNazwy.Text;                                                                     //w przeciwnym wypadku pobiera dostepna nazwe
                    portCOM.BaudRate = Convert.ToInt32(cbPredkosc.Text);                                                //konwertuje wpisana predkosc(nazwe) na liczbe 32bit
                    portCOM.DataBits = Convert.ToInt32(cbDane.Text);                                                   //konwertuje wpisana ilosc danych(nazwe) na liczbe 32bit
                    portCOM.Parity = (Parity)Enum.Parse(typeof(Parity), this.cbParzystosc.Text);                      //konwertuje mozliwe opcje parzystosci
                    portCOM.StopBits = (StopBits)Enum.Parse(typeof(StopBits), this.cbStop.Text);                     //konwertuje mozliwe opcje ilosci bitow stopu    
                    try
                    {
                        portCOM.Open();
                    }
                    catch (System.Exception ex)
                    {
                        rtbPolaczenie.Text = ex.Message;
                    }
                                                                                                                    //jezeli wsystkie okienka sa zapelnione - otwierany jest prot
                    rtbPolaczenie.Text = "Ustanowiono połączenie!";                                               //wyswietlenie statusu polaczenia
                    pbStatus.BackColor = System.Drawing.Color.Green;                   
                }
            }
        }
        private void bZamknij_Click(object sender, EventArgs e)                   //button "zamknij port" - zamyka port; czyscli konsole; czysi wszystkie okienka
        {
            portCOM.Close();
            rtbPolaczenie.Text = "Port COM został zamknięty.";
            pbStatus.BackColor = System.Drawing.Color.Red;
            this.cbNazwy.Text = "";
            this.cbPredkosc.Text = "";
            this.cbDane.Text = "";
            this.cbParzystosc.Text = "";
            this.cbStop.Text = "";
            rtbKONSOLA.Clear();
          
        }
        private void bDomyslne_Click(object sender, EventArgs e)          //button "ustawienia domyślne" - przypisanie do ustawien domyslnych ponizszych ustawien
        {
            this.cbNazwy.Text = "COM3";
            this.cbPredkosc.Text = "115200";
            this.cbDane.Text = "8";
            this.cbParzystosc.Text = "None";
            this.cbStop.Text = "One";
        }       
                        
        //ODCZYT DANYCH Z UART
        private bool startTEMPclick = false;                  //odczyt Temperatury
        private bool startNAPclick = false;                  //odczyt Napiecia-potencjometr
        private bool startSIECclick = false;                //odczyt Napiecia-sieć
        private bool diody = false;                        //odczyt stanu diody
        private bool osX = false;                         //odczyt akcelerometr

        //########################################################### ODBIERANIE DANYCH Z STM32 #######################################################################################
        private void serialPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            if (portCOM.IsOpen)
            {
                try
                {
                    string odczyt = portCOM.ReadLine().ToString();                   //Odczyt liniowy bufora oraz konwersja do postaci łańcuha znaków                                             
                    this.BeginInvoke(new LineReceivedEvent(LineReceived), odczyt);  //
                }
                catch (Exception)
                {                
                }
            }
        }
   private delegate void LineReceivedEvent(string odczyt);
        private void LineReceived(string odczyt)                                      //Wyświetlanie wiadomości w poszczególnych TextBoxach i RichTextBoxach
        {
            if (osX)
            {
                rtbPOMOC.Text = odczyt.ToString();                          //zapis odebranej wiaodmości do pomocniczego pola tekstowego
                string value = rtbPOMOC.Text;                               //zapis do dodatkowego łańcucha 
                char[] limit = new char[] { '[', ']' };                    //określenie miejsc podziałów
                string[] czesc = value.Split(limit, StringSplitOptions.RemoveEmptyEntries);     //dokonanie podziałów 
                for (int i = 0; i < czesc.Length; i++)
                {
                    rtbX.Text = czesc[0].ToString();                       //zapis podciągów do poszczególnych pól tekstowych
                    rtbY.Text = czesc[1].ToString();
                    rtbZ.Text = czesc[2].ToString();
                }
                chart5.Series["OŚ_X"].Points.AddXY(rt, rtbX.Text);      //utworzenie wykresów
                chart5.Series["OŚ_Y"].Points.AddXY(rt, rtbY.Text);
                chart5.Series["OŚ_Z"].Points.AddXY(rt, rtbZ.Text);
            }
            if (startNAPclick)
            {
                rtbNAP.Text = odczyt.ToString();
            }
            if (startSIECclick)
            {
                rtbSIEC.Text = odczyt.ToString();
            }                     
            if (startTEMPclick)
            {
                try
                {
                    tbTEMP.Text = odczyt.ToString();                                //zapisanie wartości odczytanych do pola tekstowego tmeperatury
                    verticalProgressBar1.Value = int.Parse(odczyt.ToString());     //wywietlenie temp na progress barze to jest termometrze
                }
                catch (Exception)
                {
                }
            }
            if (diody)
            {
                rtbKONSOLA.AppendText(odczyt);
            }
           
        
                   
        }

        //########################################################### STEROWANIE DIODAMI #######################################################################################

// DIODA NIEBIESKA
        private void bON1_Click(object sender, EventArgs e)
        {
            if (portCOM.IsOpen)                                          //jezeli portCOM jest otwarty
            {
                diody = true;                                           //oczyt stanu DIODY
                startNAPclick = false;                                 //resetowanie odczytu innych funkcji
                startTEMPclick = false;
                osX = false;
                startSIECclick = false;
                portCOM.Write("11");                                 //wystawia na port wartość "11"-zapal diode
                pbLED_BLUE.BackColor = System.Drawing.Color.Blue;   //zmienia kolor PictureBoxa, imitacja zapalenia niebieskiej diody            
            }
            else
            {
                rtbPolaczenie.Text = "Wybierz ustawienia portu!";
            }
        }
        private void bOFF1_Click(object sender, EventArgs e)
        {
            if (portCOM.IsOpen)                                              //jezeli portCOM jest otwarty
            {
                diody = true;
                startNAPclick = false;
                startTEMPclick = false;
                osX = false;
                startSIECclick = false;
                portCOM.Write("12");                                     //wystawia na port wartość "12"-zgas diode
                pbLED_BLUE.BackColor = System.Drawing.Color.DarkBlue;   //zmienia kolor PictureBoxa, imitacja gaszenia niebieskiej diody
            }
            else
            {
                rtbPolaczenie.Text = "Wybierz ustawienia portu!";
            }
        }

        private void bON2_Click(object sender, EventArgs e)
        {
            if (portCOM.IsOpen)
            {
                diody = true;
                startNAPclick = false;
                startTEMPclick = false;
                osX = false;
                startSIECclick = false;
                portCOM.Write("13");

                pbLED_RED.BackColor = System.Drawing.Color.Red;

            }
            else
            {
                rtbPolaczenie.Text = "Wybierz ustawienia portu!";
            }
        }

        private void bOFF2_Click(object sender, EventArgs e)
        {
            if (portCOM.IsOpen)
            {
                diody = true;
                startNAPclick = false;
                startTEMPclick = false;
                osX = false;
                startSIECclick = false;
                portCOM.Write("14");

                pbLED_RED.BackColor = System.Drawing.Color.DarkRed;
            }
            else
            {
                rtbPolaczenie.Text = "Wybierz ustawienia portu!";
            }
        }

        private void bON3_Click(object sender, EventArgs e)
        {
            if (portCOM.IsOpen)
            {
                diody = true;
                startNAPclick = false;
                startTEMPclick = false;
                osX = false;
                startSIECclick = false;
                portCOM.Write("15");

                pbLED_ORANGE.BackColor = System.Drawing.Color.Orange;
            }
            else
            {
                rtbPolaczenie.Text = "Wybierz ustawienia portu!";
            }
        }

        private void bOFF3_Click(object sender, EventArgs e)
        {
            if (portCOM.IsOpen)
            {
                diody = true;
                startNAPclick = false;
                startTEMPclick = false;
                osX = false;
                startSIECclick = false;
                portCOM.Write("16");

                pbLED_ORANGE.BackColor = System.Drawing.Color.DarkOrange;

            }
            else
            {
                rtbPolaczenie.Text = "Wybierz ustawienia portu!";
            }
        }

        private void bON4_Click(object sender, EventArgs e)
        {
            if (portCOM.IsOpen)
            {
                diody = true;
                startNAPclick = false;
                startTEMPclick = false;
                osX = false;
                startSIECclick = false;
                portCOM.Write("17");

                pbLED_GREEN.BackColor = System.Drawing.Color.Lime;
            }
            else
            {
                rtbPolaczenie.Text = "Wybierz ustawienia portu!";
            }
        }

        private void bOFF4_Click(object sender, EventArgs e)
        {
            if (portCOM.IsOpen)
            {
                startNAPclick = false;
                startTEMPclick = false;
                diody = true;
                osX = false;
                startSIECclick = false;
                portCOM.Write("18");

                pbLED_GREEN.BackColor = System.Drawing.Color.DarkGreen;
            }
            else
            {
                rtbPolaczenie.Text = "Wybierz ustawienia portu!";
            }
        }

        private void bON5_Click(object sender, EventArgs e)
        {
            if (portCOM.IsOpen)
            {
                diody = true;
                startNAPclick = false;
                startTEMPclick = false;
                osX = false;
                startSIECclick = false;
                portCOM.Write("19");

                LED1.BackColor = System.Drawing.Color.Red;
            }
            else
            {
                rtbPolaczenie.Text = "Wybierz ustawienia portu!";
            }
        }


        private void bOFF5_Click(object sender, EventArgs e)
        {
            if (portCOM.IsOpen)
            {
                diody = true;
                startNAPclick = false;
                startTEMPclick = false;
                osX = false;
                startSIECclick = false;
                portCOM.Write("20");

                LED1.BackColor = System.Drawing.Color.DarkRed;
            }
            else
            {
                rtbPolaczenie.Text = "Wybierz ustawienia portu!";
            }
        }



        private void bON6_Click(object sender, EventArgs e)
        {
            if (portCOM.IsOpen)
            {
                diody = true;
                startNAPclick = false;
                startTEMPclick = false;
                osX = false;
                startSIECclick = false;
                portCOM.Write("21");

                LED2.BackColor = System.Drawing.Color.Red;
            }
            else
            {
                rtbPolaczenie.Text = "Wybierz ustawienia portu!";
            }
        }

        private void bOFF6_Click(object sender, EventArgs e)
        {
            if (portCOM.IsOpen)
            {
                diody = true;
                startNAPclick = false;
                startTEMPclick = false;
                osX = false;
                startSIECclick = false;
                portCOM.Write("22");

                LED2.BackColor = System.Drawing.Color.DarkRed;
            }
            else
            {
                rtbPolaczenie.Text = "Wybierz ustawienia portu!";
            }
        }

        private void bON7_Click(object sender, EventArgs e)
        {
            if (portCOM.IsOpen)
            {
                diody = true;
                startNAPclick = false;
                startTEMPclick = false;
                osX = false;
                startSIECclick = false;
                portCOM.Write("23");

                LED3.BackColor = System.Drawing.Color.Red;
            }
            else
            {
                rtbPolaczenie.Text = "Wybierz ustawienia portu!";
            }
        }

        private void bOFF7_Click(object sender, EventArgs e)
        {
            if (portCOM.IsOpen)
            {
                diody = true;
                startNAPclick = false;
                startTEMPclick = false;
                osX = false;
                startSIECclick = false;
                portCOM.Write("24");

                LED3.BackColor = System.Drawing.Color.DarkRed;
            }
            else
            {
                rtbPolaczenie.Text = "Wybierz ustawienia portu!";
            }
        }

        private void bON8_Click(object sender, EventArgs e)
        {
            if (portCOM.IsOpen)
            {
                diody = true;
                startNAPclick = false;
                startTEMPclick = false;
                osX = false;
                startSIECclick = false;
                portCOM.Write("25");

                LED4.BackColor = System.Drawing.Color.Red;
            }
            else
            {
                rtbPolaczenie.Text = "Wybierz ustawienia portu!";
            }
        }

        private void bOFF8_Click(object sender, EventArgs e)
        {
            if (portCOM.IsOpen)
            {
                diody = true;
                startNAPclick = false;
                startTEMPclick = false;
                osX = false;
                startSIECclick = false;
                portCOM.Write("26");

                LED4.BackColor = System.Drawing.Color.DarkRed;
            }
            else
            {
                rtbPolaczenie.Text = "Wybierz ustawienia portu!";
            }
        }

        private void bON9_Click(object sender, EventArgs e)
        {
            if (portCOM.IsOpen)
            {
                diody = true;
                startNAPclick = false;
                startTEMPclick = false;
                osX = false;
                startSIECclick = false;
                portCOM.Write("27");

                LED5.BackColor = System.Drawing.Color.Red;
            }
            else
            {
                rtbPolaczenie.Text = "Wybierz ustawienia portu!";
            }
        }

        private void bOFF9_Click(object sender, EventArgs e)
        {
            if (portCOM.IsOpen)
            {
                diody = true;
                startNAPclick = false;
                startTEMPclick = false;
                osX = false;
                startSIECclick = false;
                portCOM.Write("28");

                LED5.BackColor = System.Drawing.Color.DarkRed;
            }
            else
            {
                rtbPolaczenie.Text = "Wybierz ustawienia portu!";
            }
        }


        private void bWyczysc_Click(object sender, EventArgs e)         //button "wyczysc" czysci konsole
        {
            rtbKONSOLA.Clear();
        }


        //########################################################### POMIAR TEMPERATURY #######################################################################################
        double realTime = 0;
        private void pomiarTEMP_Tick(object sender, EventArgs e)      //deklaracja timera
        {
            realTime = realTime + 0.1;                               //ustanowienie skoku wartosci osi odecietych (czasu)
            chart1.Series[0].Points.AddXY(realTime, tbTEMP.Text);
        }
        private void start_Click(object sender, EventArgs e)       //aktywacja pomiaru temp ADC
        {
            if (portCOM.IsOpen)
            {                                     
                pomiarTEMP.Start();                              //aktywaja timera
                rtbKONSOLA.Text = "Rozpoczęto pomiar temperatury wewnątrz mikrokontrolera.\n\r"; //komentarz w konsoli głównej
                startTEMPclick = true;                          //uruchomienie wyświetlania odczytywanej temperatury            
                diody = false;                                 //resetowanie odczytu pozostłych funkcji
                startNAPclick = false;
                osX = false;
                startSIECclick = false;
                portCOM.Write("99");                            //wysyłanie instruckji aktywujacej konwerter ADC1
            }
        }
        private void bZAPISZ_Click(object sender, EventArgs e)          //zapisanie wygenerowanego wykresu do pliku .png
        {
            chart1.SaveImage("D:\\WykresTemperatury.png", ChartImageFormat.Png);
            rtbKONSOLA.Text = "Zapisano wykres temperatury na dysku D";
        }
        private void stopTEMP_Click(object sender, EventArgs e)         //zatrzymanie timera
        {
            if (portCOM.IsOpen)
            {
                portCOM.Write("98");                                //wysyłanie instruckji dezaktywującej konwerter ADC1
                pomiarTEMP.Stop();                                 //zatrzymanie timera
                tbTEMP.Text = "0";                                //resetowanie wartości pola tekstowego temperatury   
                rtbKONSOLA.Text = "Zakończono pomiar temperatury wewnątrz mikrokontrolera.\n\r";//komentarz w konsoli głównej
                startTEMPclick = false;                         //resetowanie odczytu wszystkich funkcji
                diody = false;
                startNAPclick = false;
                osX = false;
                startSIECclick = false;
                verticalProgressBar1.Value = 0;             //resetowanie wartości paska stanu
            }
        }
        //########################################################### POMIAR NAPIĘCIA PIN #######################################################################################
        double RT = 0;
        private void czasNAP_Tick(object sender, EventArgs e)               //deklaracja timera
        {   
            RT = RT + 0.1;                                                 //ustanowienie skoku wartosci osi odecietych (czasu)
            chart2.Series["U_pin [V]"].Points.AddXY(RT, rtbNAP.Text);     //dodanie wartości do osi odciętych oraz rzędnych
        }
        private void startNAP_Click(object sender, EventArgs e)         //aktywacja pomiaru napiecia_PIN ADC
        {
            if (portCOM.IsOpen)
            {
                portCOM.Write("97");                      //wysyłanie instruckji aktywujacej konwerter ADC3     
                startNAPclick = true;                    //uruchomienie wyświetlania odczytywanego napięcia_PIN  
                diody = false;                          //resetowanie odczytu pozostłych funkcji
                startTEMPclick = false;
                osX = false;
                startSIECclick = false;
                czasNAP.Start();                     //aktywacja timer
                rtbKONSOLA.Text = "Rozpoczęto pomiar napięcia na pinie.\n\r";   //komentarz wyświetlany w konsoli głównej
            }
        }
        private void stopNAP_Click(object sender, EventArgs e)              //zatrzymanie pomiaru napięcia_PIN
        {
            if (portCOM.IsOpen)
            {
                portCOM.Write("96");                                    //wysyłanie instruckji dezaktywującej konwerter ADC3
                czasNAP.Stop();                                             //zatrzymanie timera
                rtbNAP.Text = "0";                                     //resetowanie wartości pola tekstowego napiecia   
                rtbKONSOLA.Clear();                                   //czyszczenie konsoli głównej
                rtbKONSOLA.Text = "Zakończono pomiar napięcia na pinie.\n\r";//komentarz wyświetlany w konsoli głównej
                startNAPclick = false;                                  //resetowanie odczytu wszystkich funkcji
                diody = false;
                startTEMPclick = false;
                osX = false;
                startSIECclick = false;                             
            }
        }
        private void zapiszNAP_Click(object sender, EventArgs e)
        {
            chart2.SaveImage("D:\\WykresNapieciaPIN.png", ChartImageFormat.Png); //zapisanie wygenerowanego wykresu do pliku .png
            rtbKONSOLA.Text = "Zapisano wykres napiecia PIN na dysku D";
        }

        //########################################################### POMIAR NAPIĘCIA SIECIOWEGO #######################################################################################
        double Rt = 0;
        private void siec_Tick(object sender, EventArgs e)           //deklaracja timera
        {
            Rt = Rt + 0.1;                                        //ustanowienie skoku wartosci osi odecietych (czasu)
            chart4.Series[0].Points.AddXY(Rt, rtbSIEC.Text);        //dodanie wartości do osi odciętych oraz rzędnych
        }
        private void startSIEC_Click(object sender, EventArgs e)
        {
            if (portCOM.IsOpen)
            {
                portCOM.Write("95");                            //wysyłanie instruckji aktywujacej konwerter ADC2 
                startSIECclick = true;                         //uruchomienie wyświetlania odczytywanego napięcia-sieciowego
                startNAPclick = false;                        //resetowanie odczytu pozostałych funkcji
                diody = false;
                startTEMPclick = false;
                osX = false;
                siec.Start();                               //aktywacja timera
                rtbKONSOLA.Text = "Rozpoczęto pomiar napięcia sieciowego.\n\r"; //komentarz wyświetlany w konsoli głównej
            }
        }    
        private void stopSIEC_Click(object sender, EventArgs e)         //zatrzymanie pomiaru napięcia_sieciowego
        {
            if (portCOM.IsOpen)
            {
                portCOM.Write("94");                                     //wysyłanie instruckji dezaktywującej konwerter ADC2
                rtbKONSOLA.Clear();                                     //czyszczenie konsoli głównej
                rtbKONSOLA.Text = "Zakończono pomiar napięcia.\n\r";    //komentarz wyświetlany w konsoli głównej
                startNAPclick = false;                                  //resetowanie odczytu wszystkich funkcji
                diody = false;
                startTEMPclick = false;
                osX = false;
                startSIECclick = false;
                siec.Stop();                                //zatrzymanie timera
                rtbSIEC.Text = "0";                        //resetowanie wartości pola tekstowego napiecia
            }
        }
        private void zapiszSIEC_Click(object sender, EventArgs e)   //zapisanie wygenerowanego wykresu do pliku .png
        {
            chart4.SaveImage("D:\\WykresNapieciaSieciowego.png", ChartImageFormat.Png); 
            rtbKONSOLA.Text = "Zapisano wykres napiecia sieciowego na dysku D";
        }
        //########################################################### AKCELEROMETR #######################################################################################
        double rt = 0;
        private void Akcelerometr_Tick(object sender, EventArgs e)      //deklaracja timera
        {
            if (portCOM.IsOpen)
            {
                portCOM.Write("93");                             //wysyłanie instruckji odpytującej akcelerometr
                rt = rt + 0.1;                                  //ustanowienie skoku wartosci osi odecietych (czasu)
            }
        }
        private void startAKC_Click(object sender, EventArgs e)
        {
            if (portCOM.IsOpen)
            {
                osX = true;                     //uruchomienie wyświetlania odczytywanegych przyspieszen liniowych
                startNAPclick = false;         //resetowanie odczytu pozostałych funkcji
                diody = false;
                startTEMPclick = false;               
                startSIECclick = false;
                rtbKONSOLA.Text = "Uruchomiono rejestrację wyników z akcelerometru.\n\r";   //komentarz wyświetlany w konsoli głównej
                gOSX.Start();                   //aktywacja timera
            }
        }
     
        private void zapiszAKC_Click(object sender, EventArgs e)            //zapisanie wygenerowanego wykresu do pliku .png
        {
            chart5.SaveImage("D:\\WykresAkcelerometr.png", ChartImageFormat.Png); 
            rtbKONSOLA.Text = "Zapisano wykres przyspieszen liniowych w czasie na dysku D";
        }
        private void stopAKC_Click(object sender, EventArgs e)
        {
            if (portCOM.IsOpen)
            {              
                gOSX.Stop();                                         //zatrzymanie timera
                rtbKONSOLA.Text = "Zakończono rejestrację wyników z akcelerometru.";    //komentarz wyświetlany w konsoli głównej
                rtbX.Text = "0";                        //resetowanie wartości pól tekstowych
                rtbY.Text = "0";
                rtbZ.Text = "0";
                osX = false;                        //resetowanie odczytu pozostałych funkcji
                startSIECclick = false;
                diody = false;
                startNAPclick = false;
                startTEMPclick = false;
            }
        }

        private void panel6_Paint(object sender, PaintEventArgs e)
        {

        }
    }
    }
