//Book Ticket
function changeTickets(value) {

    const input = document.getElementById("ticketCount");

    let count = parseInt(input.value);

    count += value;

    if (count < 1) count = 1;
    if (count > 100) count = 100;

    input.value = count;
}