# Simple MAUI Budgeting app

Simple android-targeting budgeting app made with MAUI.

## Features

### Custom Categories

![Categories overview and creation windows](Graphics/categories.png)

### Graphing of expenses based on selected timeframes

![Expenses Graphs](Graphics/graphs.png)

### Android: Automatic reading & parsing of google pay notifications

![Example of a parsed google play notification](Graphics/notifications.png)

## Implementation details

- [LiveCharts2](https://livecharts.dev/) to plot expenses
- [LiteDB](https://github.com/litedb-org/LiteDB) to store data on-device
- Follows the MVVM design pattern through Bindings

## Missing features

- Dark mode support
- Multiple currencies
- Removal & edit of expenses