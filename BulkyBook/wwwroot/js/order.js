var dataTable;

$(document).ready(function () {
    var url = window.location.search;
    if (url.includes("inprocess"))
        loadDataTable("GetOrderList?status=inprocess");
    else if (url.includes("pending"))
        loadDataTable("GetOrderList?status=pending");
    else if (url.includes("completed"))
        loadDataTable("GetOrderList?status=completed");
    else if (url.includes("rejected"))
        loadDataTable("GetOrderList?status=rejected");
    else if (url.includes("all"))
        loadDataTable("GetOrderList?status=all");
})

function loadDataTable(url) {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Admin/order/" + url,
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            { "data": "id", "width": "15%" },
            { "data": "name", "width": "15%" },
            { "data": "phoneNumber", "width": "15%" },
            { "data": "applicationUser.email", "width": "15%" },
            { "data": "orderStatus", "width": "15%" },
            { "data": "orderTotal", "width": "15%" },
            {
                "data": "id",
                "render": function (data) {
                    return `<div class="text-center">
                                <a href="/Admin/Order/Details/${data}" class="btn-success" style="cursor:pointer">    <i class="fas fa-user-edit"></i> &nbsp;</a>
                             </div>`
                },
                "width": "5%"
            }
        ],
        "language": {
            "emptyTable": "No data found"
        },
        "width": "100%"
    })
}

function Delete(url) {
    swal({
        title: "Are you sure?",
        text: "Once deleted, you will not be able to recover.",
        icon: "warning",
        dangerMode: true,
        buttons: true
    }).then((willDelete) => {
        if (willDelete) {
            $.ajax({
                type: "DELETE",
                url: url,
                success: function (data) {
                    if (data.success) {
                        toastr.success(data.message);
                        dataTable.ajax.reload();
                    }
                    else {
                        toastr.error(data.message);
                    }
                }
            })
        }
    })
}