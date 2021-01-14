var dataTable;

$(document).ready(function () {
    loadDataTable();
})

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Admin/User/GetAll",
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            { "data": "name", "width": "15%" },
            { "data": "email", "width": "15%" },
            { "data": "phoneNumber", "width": "15%" },
            { "data": "company.name", "width": "15%" },
            { "data": "role", "width": "15%" },
            {
                "data": {id: "id", lockOutEnd : "lockoutEnd"},
                "render": function (data) {
                    var today = new Date().getTime();
                    var lockout = new Date(data.lockOutEnd).getTime();
                    if (lockout > today) {
                        //user is locked
                        return `<div class="text-center">
                                <a onclick=LockUnlock("${data.id}") class="btn btn-danger text-white" style="cursor:pointer">    <i class="fas fa-lock-open"></i> Unlock </a>
                            </div>`
                    }
                    else {
                        return `<div class="text-center">
                                <a onclick=LockUnlock("${data.id}") class="btn btn-success text-white" style="cursor:pointer">    <i class="fas fa-lock"></i> Lock </a>
                            </div>`
                    }
                  
                },
                "width": "40%"
            }
        ],
        "language": {
            "emptyTable": "No data found"
        },
        "width": "100%"
    })
}

function LockUnlock(id) {
    $.ajax({
        type: "POST",
        url: '/Admin/User/LockUnlockUser',
        data: JSON.stringify(id),
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
    })
}