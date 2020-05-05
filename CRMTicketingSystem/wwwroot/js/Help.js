var dataTable;

$(document).ready(function () {
    loadDataTable();
});


function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Admin/Help/GetAll"
        },
        "columns": [
            { "data": "subject", "width": "10%" },
            { "data": "email", "width": "20%" },
            { "data": "description", "width": "15%" },
            { "data": "createdDate", "width": "15%" },
            {
                "data": "id",
                "render": function (data) {
                    return `
                            <div class="text-center">
                                <a href="/Admin/Help/Review/${data}" class="btn btn-success text-white" style="cursor:pointer">
                                    <i class="fas fa-edit"></i> Review
                                </a>                               
                            </div>                           
                           `;
                }, "width": "20%"
            },
            {
                
                "data": {
                    id: "id", ticketStatus: "ticketStatus"
                },
                "render": function (data) {
                    var ticketStatus =data.ticketStatus;
                    if (ticketStatus == 9) {
                        return `
                            <div class="text-center">
                                <a onclick=Resolve('${data.id}') class="btn btn-danger text-white" style="cursor:pointer; width:100px;">
                                    <i class="fas fa-edit"></i> Resolved
                                </a>
                            </div>
                           `;
                    }
                    else {
                        return `
                            <div class="text-center">
                                <a onclick=Resolve('${data.id}') class="btn btn-success text-white" style="cursor:pointer; width:100px;">
                                    <i class="fas fa-edit"></i> Resolve
                                </a>
                            </div>
                           `;
                    }

                },
                "width": "20%"
            }
        ]
    });
}

function Resolve(id) {

    $.ajax({
        type: "GET",
        url: '/Admin/Help/Resolve/' + id,
        contentType: "application/json",
        success: function (data) {
            if (data.success) {
                toastr.success(data.message);
                dataTable.ajax.reload();
            }
            else {
                toastr.error(data.message);
            }
        }
    });

}