<template>
    <div class="home">

        <div class="card-tile" >
            <router-link to="/kafka" >
            <span>
                Manage Kafka
            </span>
        </router-link>
        </div>

        <div class="card-tile">
            <span>
                Manage RabbitMQ
            </span>
        </div>
        <div class="card-tile">
            <span>
                Manage Redis
            </span>
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
