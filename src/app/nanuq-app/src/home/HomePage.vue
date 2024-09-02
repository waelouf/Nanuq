<template>
    <div class="row">
        <span class="mt-4"></span>
        <ol class="breadcrumb mb-4">
            <li class="breadcrumb-item active"></li>
        </ol>
        <div class="col-xl-3 col-md-6">
            <div class="card bg-primary text-white mb-4">
                <div class="card-body">Manage Kafka</div>
                <router-link to="/kafka" >
                <div class="card-footer d-flex align-items-center justify-content-between">
                    <span class="small text-white stretched-link" href="#"></span>
                    <div class="small text-white"><i class="fas fa-angle-right"></i></div>
                </div>
            </router-link>
            </div>
        </div>

        <div class="col-xl-3 col-md-6">
            <div class="card bg-primary text-white mb-4">
                <div class="card-body">Manage Redis</div>
                <router-link to="/redis" >
                <div class="card-footer d-flex align-items-center justify-content-between">
                    <span class="small text-white stretched-link" href="#"></span>
                    <div class="small text-white"><i class="fas fa-angle-right"></i></div>
                </div>
            </router-link>
            </div>
        </div>

        <div class="col-xl-3 col-md-6" style="display:none;" >
            <div class="card bg-primary text-white mb-4">
                <div class="card-body">Manage RabbitMQ</div>
                <router-link to="/rabbitmq" >
                <div class="card-footer d-flex align-items-center justify-content-between">
                    <span class="small text-white stretched-link" href="#"></span>
                    <div class="small text-white"><i class="fas fa-angle-right"></i></div>
                </div>
            </router-link>
            </div>
        </div>

    </div>
</template>
<script>
export default {

};
</script>
<style>
.home{
    display: flex;
}

.card-tile {
    margin: auto;
    width: 30%;
    height: 100px;
    border-style: solid;
    border-radius: 25px;
}
.card-tile span {
    text-align: center;
    position: relative;
    font-weight: 700;
    font-size: 1.1rem;
}
</style>
