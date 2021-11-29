function generateChart(data, labels) {
    var ctx = document.getElementById("chart").getContext("2d");
    var chart = new Chart(ctx, {
        // The type of chart we want to create
        type: "line",
        responsive: true,

        // The data for our dataset
        data: {
            labels: labels,
            datasets: data,
        },

        // Configuration options go here
        options: {},
    });
}