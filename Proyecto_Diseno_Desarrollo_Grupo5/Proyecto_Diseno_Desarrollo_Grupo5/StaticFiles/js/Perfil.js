const okMsg = @Html.Raw(Newtonsoft.Json.JsonConvert.SerializeObject(ok));
const errMsg = @Html.Raw(Newtonsoft.Json.JsonConvert.SerializeObject(mensaje));

if (okMsg) {
    Swal.fire("Listo", okMsg, "success");
}
if (errMsg) {
    Swal.fire("Atención", errMsg, "warning");
}