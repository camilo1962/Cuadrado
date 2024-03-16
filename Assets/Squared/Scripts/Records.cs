using System;
using UnityEngine;
using TMPro;

namespace Squared
{
    public class Records : MonoBehaviour
    {
        public TMP_Text recordText1;
        public TMP_Text recordText2;
        public TMP_Text recordText3;
        public TMP_Text recordText4;
        public TMP_Text recordText5;
        public TMP_Text recordText6;
        public TMP_Text recordText7;
        public TMP_Text recordText8;
        public TMP_Text recordText9;
        public TMP_Text recordText10;
        public TMP_Text recordText11;
        public TMP_Text recordText12;

        public void Start()
        {
            DisplayRecord(1, recordText1);
            DisplayRecord(2, recordText2);
            DisplayRecord(3, recordText3);
            DisplayRecord(4, recordText4);
            DisplayRecord(5, recordText5);
            DisplayRecord(6, recordText6);
            DisplayRecord(7, recordText7);
            DisplayRecord(8, recordText8);
            DisplayRecord(9, recordText9);
            DisplayRecord(10, recordText10);
            DisplayRecord(11, recordText11);
            DisplayRecord(12, recordText12);
        }

        private void DisplayRecord(int recordNumber, TMP_Text recordText)
        {
            float recordInSeconds = PlayerPrefs.GetFloat("record" + recordNumber, 0);

            // Convertir los segundos a TimeSpan para obtener el formato de minutos, segundos y centésimas
            TimeSpan tiempo = TimeSpan.FromSeconds(recordInSeconds);

            // Formatear el tiempo en una cadena de texto con centésimas
            string tiempoFormateado = $"{tiempo.Minutes:00}:{tiempo.Seconds:00}:{tiempo.Milliseconds / 10:00}";

            // Establecer el texto del récord formateado
            recordText.text = tiempoFormateado;
        }
    }
}
