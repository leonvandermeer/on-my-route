﻿namespace OnMyRoute;

public partial class MainWindow : Window {
    public MainWindow(MainViewModel viewModel) {
        InitializeComponent();
        DataContext = viewModel;
    }
}
