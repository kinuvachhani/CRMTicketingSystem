var dataTable;

$(document).ready(function () {
    loadDataTable();
});


function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Customer/Ticket/GetAll"
        },
        "columns": [
            { "data": "subject", "width": "10%" },
            { "data": "product.title", "width": "10%" },
            { "data": "email", "width": "15%" },
            { "data": "description", "width": "15%" },
            { "data": "createdDate", "width": "15%" },
            { "data": "review", "width": "10%" },
            {"data":"status", "width":"10%"}
        ]
    });
}

function Resolve(id) {

    $.ajax({
        type: "GET",
        url: '/Admin/Ticket/Resolve/' + id,
        contentType: "application/json",
        success: function (data) {
            if (data.success) {
                toastr.success(data.message);
                dataTable.ajax.reload();
            }
        }
    });

}