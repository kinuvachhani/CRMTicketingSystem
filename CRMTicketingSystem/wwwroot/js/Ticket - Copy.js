var dataTable;

$(document).ready(function () {
    loadDataTable();
});


function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Admin/Ticket/GetAll"
        },
        "columns": [
            { "data": "subject", "width": "10%" },
            { "data": "product.title", "width": "10%" },
            { "data": "email", "width": "20%" },
            { "data": "description", "width": "15%" },
            { "data": "createdDate", "width": "15%" },
            {
                "data": "id",
                "render": function (data) {
                    return `
                            <div class="text-center">
                                <a href="/Admin/Ticket/Review/${data}" class="btn btn-success text-white" style="cursor:pointer">
                                    <i class="fas fa-edit"></i> Review
                                </a>                               
                            </div>                           
                           `;
                }, "width": "15%"
            },
            {
                "data": {
                    id: "id"
                },
                "render": function (data) {
                    return `
                            <div class="text-center">
                                <a onclick=Resolve('${data.id}') class="btn btn-success text-white" style="cursor:pointer">
                                    <i class="fas fa-edit"></i> Resolve
                                </a>                               
                            </div>                           
                           `;
                }, "width": "15%"
            }
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