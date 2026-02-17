document.addEventListener("DOMContentLoaded", () => {
    const modalEl = document.getElementById("modalUsuario");
    const modal = new bootstrap.Modal(modalEl);

    const btnNuevo = document.getElementById("btnNuevoUsuario");
    const btnGuardar = document.getElementById("btnGuardarUsuario");

    const uId = document.getElementById("uId");
    const uNombre = document.getElementById("uNombre");
    const uCorreo = document.getElementById("uCorreo");
    const uRol = document.getElementById("uRol");
    const uPass = document.getElementById("uPass");
    const modalTitle = document.getElementById("modalUsuarioTitle");

    const limpiar = () => {
        uId.value = 0;
        uNombre.value = "";
        uCorreo.value = "";
        uRol.value = "0";
        uPass.value = "";
    };

    const validar = (esEdicion) => {
        const nombre = (uNombre.value || "").trim();
        const correo = (uCorreo.value || "").trim();
        const rol = parseInt(uRol.value || "0", 10);
        const pass = (uPass.value || "").trim();

        if (nombre.length < 2) {
            Swal.fire("Validación", "El nombre debe tener al menos 2 caracteres.", "warning");
            return false;
        }

        if (correo.length < 5 || !correo.includes("@")) {
            Swal.fire("Validación", "Ingrese un correo válido.", "warning");
            return false;
        }

        if (rol <= 0) {
            Swal.fire("Validación", "Seleccione un rol.", "warning");
            return false;
        }

        // contraseña obligatoria al crear
        if (!esEdicion) {
            if (pass.length < 4) {
                Swal.fire("Validación", "La contraseña debe tener mínimo 4 caracteres.", "warning");
                return false;
            }
        } else {
            // en edición es opcional, pero si se escribe debe ser válida
            if (pass.length > 0 && pass.length < 4) {
                Swal.fire("Validación", "Si cambia la contraseña, debe tener mínimo 4 caracteres.", "warning");
                return false;
            }
        }

        return true;
    };

    // =========================
    // NUEVO
    // =========================
    btnNuevo.addEventListener("click", () => {
        limpiar();
        modalTitle.textContent = "Nuevo Usuario";
        modal.show();
    });

    // =========================
    // EDITAR
    // =========================
    document.querySelectorAll(".btnEditar").forEach(btn => {
        btn.addEventListener("click", () => {
            limpiar();

            uId.value = btn.dataset.id;
            uNombre.value = btn.dataset.nombre || "";
            uCorreo.value = btn.dataset.correo || "";
            uRol.value = btn.dataset.rol || "0";

            // contraseña queda vacía (solo si la quiere cambiar)
            uPass.value = "";

            modalTitle.textContent = "Editar Usuario";
            modal.show();
        });
    });

    // =========================
    // GUARDAR (CREATE/UPDATE)
    // =========================
    btnGuardar.addEventListener("click", async () => {
        const esEdicion = parseInt(uId.value || "0", 10) > 0;

        if (!validar(esEdicion)) return;

        const payload = {
            IdUsuario: parseInt(uId.value || "0", 10),
            Nombre: (uNombre.value || "").trim(),
            Correo: (uCorreo.value || "").trim(),
            IdRol: parseInt(uRol.value || "0", 10),
            Contrasena: (uPass.value || "").trim()
        };

        const url = esEdicion ? "/Usuarios/Update" : "/Usuarios/Create";

        try {
            const resp = await fetch(url, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(payload)
            });

            const data = await resp.json();

            if (data.ok) {
                await Swal.fire("Listo", esEdicion ? "Usuario actualizado." : "Usuario creado.", "success");
                window.location.reload();
            } else {
                Swal.fire("Error", data.msg || "No se pudo guardar.", "error");
            }
        } catch (e) {
            Swal.fire("Error", "Ocurrió un error al comunicarse con el servidor.", "error");
        }
    });

    // =========================
    // TOGGLE ESTADO
    // =========================
    document.querySelectorAll(".btnToggle").forEach(btn => {
        btn.addEventListener("click", async () => {
            const id = parseInt(btn.dataset.id || "0", 10);
            const estado = parseInt(btn.dataset.estado || "1", 10);
            const accion = estado === 1 ? "inactivar" : "activar";

            const confirm = await Swal.fire({
                title: "Confirmación",
                text: `¿Desea ${accion} este usuario?`,
                icon: "question",
                showCancelButton: true,
                confirmButtonText: "Sí",
                cancelButtonText: "Cancelar"
            });

            if (!confirm.isConfirmed) return;

            try {
                const resp = await fetch("/Usuarios/ToggleEstado", {
                    method: "POST",
                    headers: { "Content-Type": "application/x-www-form-urlencoded" },
                    body: `id=${encodeURIComponent(id)}`
                });

                const data = await resp.json();

                if (data.ok) {
                    await Swal.fire("Listo", "Estado actualizado.", "success");
                    window.location.reload();
                } else {
                    Swal.fire("Error", "No se pudo actualizar el estado.", "error");
                }
            } catch (e) {
                Swal.fire("Error", "Ocurrió un error al comunicarse con el servidor.", "error");
            }
        });
    });

});