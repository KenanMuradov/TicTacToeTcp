﻿using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Client;

public partial class MainWindow : Window
{
    private readonly TcpClient _client;
    private readonly NetworkStream _stream;
    private readonly BinaryReader _br;
    private readonly BinaryWriter _bw;
    private readonly Button[] _buttons;
    private bool _myTurn;
    private char _mySymbol;
    public MainWindow()
    {
        InitializeComponent();
        _client = new TcpClient("127.0.0.1", 27001);
        _stream = _client.GetStream();
        _bw = new BinaryWriter(_stream);
        _br = new BinaryReader(_stream);
        MyGrid.IsEnabled = false;

        _buttons = new Button[9]
        {
            btn1, btn2, btn3,
            btn4, btn5, btn6,
            btn7, btn8, btn9
        };


        Task.Run(GetStarted);
    }

    private void GetChanges()
    {
        while (true)
        {
            Task.Delay(500);

            if (_stream.DataAvailable)
            {
                var chars = _br.ReadChars(9);

                for (int i = 0; i < _buttons.Length; i++)
                    Dispatcher.Invoke(() => _buttons[i].Content = chars[i]);

                _myTurn = !_myTurn;
                Dispatcher.Invoke(() => MyGrid.IsEnabled = _myTurn);
                CheckForWinner(chars);

            }
        }
    }

    private void GetStarted()
    {
        _myTurn = _br.ReadBoolean();
        _mySymbol = _br.ReadChar();
        Dispatcher.Invoke(() => MyGrid.IsEnabled = _myTurn);
        Task.Run(GetChanges);
    }

    private void CheckForWinner(char[] chars)
    {
        const int arraySize = 3;

        char[,] tmp = new char[arraySize, arraySize]
        {
            {chars[0],chars[1],chars[2] },
            {chars[3],chars[4],chars[5] },
            {chars[6],chars[7],chars[8] },
        };


        for (int i = 0; i < arraySize; i++)
        {
            if (tmp[i, 0] == tmp[i, 1] && tmp[i, 1] == tmp[i, 2] && tmp[i, 0] == _mySymbol)
            {
                MessageBox.Show($"{_mySymbol} Won");
                ClearDesk();
                return;
            }

            if (tmp[0, i] == tmp[1, i] && tmp[1, i] == tmp[2, i] && tmp[0, i] == _mySymbol)
            {
                MessageBox.Show($"{_mySymbol} Won");
                ClearDesk();
                return;
            }

            if (tmp[0,0] == tmp[1,1] && tmp[1,1] == tmp[2,2] && tmp[0,0] == _mySymbol)
            {
                MessageBox.Show($"{_mySymbol} Won");
                ClearDesk();
                return;
            }

            if (tmp[0,2] == tmp[1, 1] && tmp[1, 1] == tmp[2, 0] && tmp[0, 2] == _mySymbol)
            {
                MessageBox.Show($"{_mySymbol} Won");
                ClearDesk();
                return;
            }
        }

        for (int i = 0; i < arraySize; i++)
        {
            if (tmp[i, 0] == tmp[i, 1] && tmp[i, 1] == tmp[i, 2] && tmp[i, 0] != _mySymbol && tmp[i, 0]!='\0')
            {
                MessageBox.Show($"{_mySymbol} Lost");
                ClearDesk();
                return;
            }

            if (tmp[0, i] == tmp[1, i] && tmp[1, i] == tmp[2, i] && tmp[0, i] != _mySymbol && tmp[0, i] != '\0')
            {
                MessageBox.Show($"{_mySymbol} Lost");
                ClearDesk();
                return;
            }

            if (tmp[0, 0] == tmp[1, 1] && tmp[1, 1] == tmp[2, 2] && tmp[0, 0] != _mySymbol && tmp[0, 0] != '\0')
            {
                MessageBox.Show($"{_mySymbol} Lost");
                ClearDesk();
                return;
            }

            if (tmp[0, 2] == tmp[1, 1] && tmp[1, 1] == tmp[2, 0] && tmp[0, 2] != _mySymbol && tmp[0, 2] != '\0')
            {
                MessageBox.Show($"{_mySymbol} Lost");
                ClearDesk();
                return;
            }
        }

        var isTie = true;

        for (int i = 0; i < arraySize; i++)
            for (int j = 0; j < arraySize; j++)
                if (tmp[i,j] == '\0')
                    isTie = false;

        if (isTie)
        {
            MessageBox.Show($"Game ended tie");
            ClearDesk();
            return;
        }
    }

    private void ClearDesk()
    {
        foreach (var btn in _buttons)
            Dispatcher.Invoke(() => btn.Content = null);
    }

    private void btn_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            if (button.Content is null || (char)button.Content == '\0')
            {
                button.Content = _mySymbol;
                char[] chars = new char[_buttons.Length];

                for (int i = 0; i < _buttons.Length; i++)
                {
                    if (_buttons[i].Content is char c)
                        chars[i] = c;
                }

                _bw.Write(chars);
                _bw.Flush();
                _myTurn = !_myTurn;
                MyGrid.IsEnabled = _myTurn;
                CheckForWinner(chars);

            }
        }
    }
}
